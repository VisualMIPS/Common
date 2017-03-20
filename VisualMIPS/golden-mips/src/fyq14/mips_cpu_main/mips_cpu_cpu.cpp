#include "MIPS/cpu.h"
#include "MIPS/register.h"
#include "MIPS/utils.h"

// ===============================
// mips_cpu_impl class definitions
// ===============================

mips_cpu_impl::mips_cpu_impl(mips_mem_h arg_mem_handler) :
    fetcher(MIPS::InstructionFetcher(arg_mem_handler, this)),
    decoder(MIPS::InstructionDecoder(*this))
{
    mem_handler = arg_mem_handler;
    ptr_registers[0] = new MIPS::ZeroRegister();
    ptr_registers[0]->set_ptr_cpu(this);
    for (int i = 1 ; i < 32 ; i++) {
        ptr_registers[i] = new MIPS::RegularRegister();
        ptr_registers[i]->set_ptr_cpu(this);
        static_cast<MIPS::RegularRegister*>(ptr_registers[i])->set_index(i);
    }
    ptr_pc = new MIPS::RegularRegister();
    ptr_register_lo = new MIPS::RegularRegister();
    ptr_register_hi = new MIPS::RegularRegister();

    ptr_pc->set_ptr_cpu(this);
    ptr_register_lo->set_ptr_cpu(this);
    ptr_register_hi->set_ptr_cpu(this);
    static_cast<MIPS::RegularRegister*>(ptr_register_lo)
        ->set_index(MIPS::RegularRegister::ID_LO);
    static_cast<MIPS::RegularRegister*>(ptr_register_hi)
        ->set_index(MIPS::RegularRegister::ID_HI);
    static_cast<MIPS::RegularRegister*>(ptr_pc)
        ->set_index(MIPS::RegularRegister::ID_PC);

}

mips_cpu_impl::~mips_cpu_impl() {
    for (int i = 0 ; i < 32 ; i++) {
        delete ptr_registers[i];
    }
    delete ptr_pc;
    delete ptr_register_lo;
    delete ptr_register_hi;
}

uint32_t mips_cpu_impl::get_pc() {
    return ptr_pc->get();
}

void mips_cpu_impl::set_pc(uint32_t v) {
    while(!delayed_branches.empty()) {
        delayed_branches.pop();
    }
    jump(v);
}

uint32_t mips_cpu_impl::get_register(unsigned index) {
    return ptr_registers[index]->get();
}

void mips_cpu_impl::set_register(unsigned index, uint32_t value) {
    ptr_registers[index]->set(value);
}

void mips_cpu_impl::reset() {
    // Reset registers to 0
    logger.debug("=========== CPU RESET ===========\n");
    ptr_pc->set(0);
    ptr_register_lo->set(0);
    ptr_register_hi->set(0);
    for (uint32_t i = 0 ; i < 32 ; i++) {
        ptr_registers[i]->set(0);
    }

    // clear all buffers!
    while(!delayed_branches.empty()) {
        delayed_branches.pop();
    }
    decoder.clear();
    fetcher.clear();
    fetcher.jump(0);
    logger.debug("=========== END OF CPU RESET ===========\n");
}

void mips_cpu_impl::step() {
    logger.debug("======== START CPU STEP ========\n");
    MIPS::Instruction * instruction = NULL;
    instruction_t instruction_bits;

    // Run through the pipeline once
    // 1. Fetch the next instruction
    fetcher.fetch();
    // 2. Feed instruction from fetcher into decoder
    fetcher >> instruction_bits;
    decoder << instruction_bits;
    // 3. Decode the instruction
    decoder >> instruction;

    // 4. Prepare the clean up procedure (to prevent memory leak)

    auto clean_up_procedure = ([instruction]{
        if (instruction != NULL) {
            delete instruction;
        }
    });
    // auto croak_procedure = ([this]{
    //     logger.debug("======== END OF STEP ~~~WITH EXCEPTION~~~ =============\n");
    // });
    auto clean_up = FINALLY(clean_up_procedure);
    // auto log_end_of_step_with_exception = FINALLY(croak_procedure);

    // 5. Execute the instruction, with chances of Exceptions (how to handle?)
    bool has_branch_addr = 0;
    addr_t branch_addr;

    if (!delayed_branches.empty()) {
        std::pair<uint32_t, MIPS::jump_t> top = delayed_branches.front();
        has_branch_addr = true;
        delayed_branches.pop();

        switch (top.second) {
        case MIPS::J_REG:
            branch_addr = top.first;
            break;
        case MIPS::J_BRANCHES:
            branch_addr = top.first + ptr_pc->get();
            break;
        case MIPS::J_J:
            branch_addr = (top.first & 0x0FFFFFFF) |
                (ptr_pc->get() & 0xF0000000);
            break;
        }
    }
    logger.debug("    Executing at PC = 0x%.8X ...\n", ptr_pc->get());
    instruction->execute();

    if (has_branch_addr) {
        jump(branch_addr);
    } else {
        ptr_pc->set(ptr_pc->get() + 4);
    }
    logger.debug("======== SUCCESSFUL END OF STEP =============\n");
    // log_end_of_step_with_exception.disable();
}

void mips_cpu_impl::jump(addr_t addr) {
    logger.debug("    => PC is jumping to 0x%.8X\n", addr);
    ptr_pc->set(addr);
    decoder.clear();
    fetcher.jump(addr);
}

// ===============================
// mips_cpu_impl "API" definitions
// ===============================

mips_cpu_h mips_cpu_create(mips_mem_h mem) {
    return new mips_cpu_impl(mem);
}

mips_error mips_cpu_reset(mips_cpu_h state) {
    state->reset(); 
    return mips_Success;
}

mips_error mips_cpu_get_register(
    mips_cpu_h state,	        //!< Valid (non-empty) handle to a CPU
    unsigned index,		//!< Index from 0 to 31
    uint32_t *value		//!< Where to write the value to
) {
    if (!(index <= 31)) {
        return mips_ErrorInvalidArgument;
    }

    *value = state->get_register(index);
    return mips_Success;
}

mips_error mips_cpu_set_register(
    mips_cpu_h state,	        //!< Valid (non-empty) handle to a CPU
    unsigned index,		//!< Index from 0 to 31
    uint32_t value		//!< New value to write into register file
) {
    if (!(index <= 31)) {
        return mips_ErrorInvalidArgument;
    }

    state->set_register(index, value);
    return mips_Success;
}

mips_error mips_cpu_set_pc(
    mips_cpu_h state,	        //!< Valid (non-empty) handle to a CPU
    uint32_t pc			//!< Address of the next instruction to exectute.
) {
    state->set_pc(pc);

    return mips_Success;
}

mips_error mips_cpu_get_pc(
    mips_cpu_h state,	        //!< Valid (non-empty) handle to a CPU
    uint32_t *pc		//!< Where to write the byte address too
) {
    *pc = state->get_pc();
    return mips_Success;
}

mips_error mips_cpu_step(
    mips_cpu_h state	        //! Valid (non-empty) handle to a CPU
) {
    try {
        state->step();
        return mips_Success;
    } catch (MIPS::Exception e) {
        state->logger.debug(
            "=> Exception code 0x%.8X : %s\n",
            e.get_error_code(), e.to_str().c_str()
        );
        return e.get_error_code();
    }
}

mips_error mips_cpu_set_debug_level(mips_cpu_h state, unsigned level, FILE *dest) {
    state->logger.set_level(level);
    state->logger.set_fout(dest);

    return mips_Success;
}

void mips_cpu_free(mips_cpu_h state) {
    delete state;
}

