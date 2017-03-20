#include <iostream>

#include "MIPS/instruction.h"
#include "MIPS/types.h"
#include "MIPS/functions.h"

namespace MIPS {
    Instruction::~Instruction() {}

    void InvalidInstruction::execute() {
        throw MIPS::InvalidInstructionException();
    }

    void RInstruction::set_rs(RegisterInterface * r) {
        rs = r;
    }

    void RInstruction::set_rt(RegisterInterface * r) {
        rt = r;
    }

    void RInstruction::set_rd(RegisterInterface * r) {
        rd = r;
    }

    ArithmeticRInstruction::ArithmeticRInstruction(operator_t arg_fnc, bool flag) {
        fnc = arg_fnc;
        is_can_overflow = flag;
    }

    void ArithmeticRInstruction::execute() {
        uint32_t res;
        bool flag = fnc(rs->get(), rt->get(), &res);
        if (is_can_overflow && flag) {
            throw MIPS::ArithmeticOverflowException();
        } else {
            rd->set(res);
        }
    }

    ShiftRInstruction::ShiftRInstruction(shift_fnc_t fnc_in) :
        fnc(fnc_in) {}

    void ShiftRInstruction::set_shift(uint32_t shift_in) {
        shift = shift_in;
    }

    void ShiftRInstruction::execute() {
        // Note : only use the low order 5 bits
        rd->set(fnc(rt->get(), (rs->get() + shift) & 0x1F));
    }

    MultDivInstruction::MultDivInstruction(
        operator_t fnc_lo_in,
        operator_t fnc_hi_in
    ) {
        fnc_lo = fnc_lo_in;
        fnc_hi = fnc_hi_in;
    }

    void MultDivInstruction::set_r_lo(RegisterInterface * ptr_r_lo_in) {
        ptr_r_lo = ptr_r_lo_in;
    }

    void MultDivInstruction::set_r_hi(RegisterInterface * ptr_r_hi_in) {
        ptr_r_hi = ptr_r_hi_in;
    }

    void MultDivInstruction::execute() {
        uint32_t res_lo, res_hi;
        fnc_lo(rs->get(), rt->get(), &res_lo);
        fnc_hi(rs->get(), rt->get(), &res_hi);
        ptr_r_lo->set(res_lo);
        ptr_r_hi->set(res_hi);
    }

    JumpRInstruction::JumpRInstruction(
        std::queue<std::pair<uint32_t, MIPS::jump_t>> * arg) :
        ptr_delayed_branches(arg) {}

    void JumpRInstruction::execute() {
        ptr_delayed_branches->push(
            make_pair(rs->get(), MIPS::J_REG)
        );
    }

    void JumpLinkRInstruction::set_pc(RegisterInterface * ptr_pc_in) {
        ptr_pc = ptr_pc_in;
    }

    void JumpLinkRInstruction::execute() {
        JumpRInstruction::execute();
        rd->set(ptr_pc->get() + 8);
    }

    // ========================================
    // IInstruction
    // ========================================

    IInstruction::IInstruction() {
        
    }

    void IInstruction::set_rs(RegisterInterface* ptr_rs_in) {
        ptr_rs = ptr_rs_in;
    }

    void IInstruction::set_rt(RegisterInterface* ptr_rt_in) {
        ptr_rt = ptr_rt_in;
    }

    void IInstruction::set_konst(uint32_t konst_in) {
        konst = konst_in;
    }

    BitwiseIInstruction::BitwiseIInstruction(operator_t fnc_in) {
        fnc = fnc_in;
    }

    void BitwiseIInstruction::execute() {
        uint32_t res;
        fnc(ptr_rs->get(), konst, &res);
        ptr_rt->set(res);
    }

    ArithmeticIInstruction::ArithmeticIInstruction(operator_t fnc_in, bool flag) {
        fnc = fnc_in;
        is_can_overflow = flag;
    }

    void ArithmeticIInstruction::execute() {
        uint32_t res;
        bool flag = fnc(ptr_rs->get(), functions::sign_extend<16>(konst), &res);

        if (is_can_overflow && flag) {
            throw ArithmeticOverflowException();
        } else {
            ptr_rt->set(res);
        }
    }

    template <bool is_write, uint32_t n_bytes, bool is_signed>
    MemoryReadWriteIInstruction<is_write, n_bytes, is_signed>::MemoryReadWriteIInstruction(
        mips_mem_h ptr_mem_in 
    ) : ptr_mem(ptr_mem_in) {}

    template <bool is_write, uint32_t n_bytes, bool is_signed>
    uint32_t MemoryReadWriteIInstruction<is_write, n_bytes, is_signed>::get_addr(){
        return ptr_rs->get() + functions::sign_extend<16>(konst);
    }

    template <bool is_write, uint32_t n_bytes, bool is_signed>
    uint32_t MemoryReadWriteIInstruction<is_write, n_bytes,is_signed>::sign_extend(uint32_t arg) {
        if (is_signed) {
            return functions::sign_extend<n_bytes * 8>(arg);
        } else {
            return arg;
        }
    }

    template <bool is_write, uint32_t n_bytes, bool is_signed>
    uint32_t MemoryReadWriteIInstruction<is_write, n_bytes, is_signed>::get_mask() {
        uint32_t output = 0;
        for (uint32_t i = 0; i < n_bytes ; i++) {
            output |= (0xFF << (i * 8));
        }

        return output;
    }

    template <bool is_write, uint32_t n_bytes, bool is_signed>
    void MemoryReadWriteIInstruction<is_write, n_bytes,is_signed>::execute() {
        uint32_t addr = get_addr(), log2;
        // the last (n_bytes - 1) bits of the effective addres must be zero
        // emulate "log2 for int"
        switch(n_bytes) {
            case 1:  log2 = 0; break;
            case 2:  log2 = 1; break;
            case 4:  log2 = 2; break;
            default: throw InvalidInstructionException();
        }

        for (uint32_t i = 0 ; i < log2 ; i++) {
            if (((addr >> i) & 0x1) != 0) {
                throw InvalidAlignmentException();
            }
        }

        if (is_write) {
            // how write?
            // addr is the actual address of the stuff we want to write
            // start_addr is the address we want to query the memory
            uint32_t offset = addr % 4,
                     start_addr = addr - offset;
            uint8_t buffer[8];
            uint8_t data_buffer[n_bytes];
            uint32_t mask = get_mask();
            uint32_t data = ptr_rt->get() & mask;

            for (uint32_t i = 0 ; i < n_bytes ; i++) {
                data_buffer[i] = (data >> (8 * (n_bytes - i - 1))) & 0xFF;
            }

            WITH_CHECK_ERROR(
                mips_mem_read(
                    ptr_mem, start_addr, 8, buffer
                )
            );

            // Populate buffer with data_buffer
            for (uint32_t i = 0 ; i < n_bytes ; i++) {
                buffer[offset + i] = data_buffer[i];
            }

            // If this fails, throws an error, and state of processor / memory
            // would not have been modified
            WITH_CHECK_ERROR(
                mips_mem_write(
                    ptr_mem, start_addr, 8, buffer
                )
            );

        } else {
            // read
            uint8_t data[n_bytes];
            uint8_t buffer[8];
            uint32_t output;
            uint32_t offset = (addr % 4),
                     read_addr = addr - offset;

            // Memory read might throw and error - but it is fine, we can safely
            // let it do it's stuff

            // 1. Read from memory (throw exception if needed)
            WITH_CHECK_ERROR(
                mips_mem_read(
                    ptr_mem, read_addr, 8, buffer 
                )
            );

            // ===============
            // NOTE : TODO : PLATFORM DEPENDENT, ASSUMING ON A LITTLE ENDIAN HOST
            // ===============

            // 2. load into data
            for (uint32_t i = 0 ; i < n_bytes ; i++) {
                data[i] = buffer[offset + i];
            }

            // 3. swap endianness
            // Gosh the type casting :(
            output = 0;
            for (uint32_t i = 0 ; i < n_bytes ; i++) {
                output |= ((uint32_t(data[i]) & 0xFF) << ((n_bytes - 1 - i) * 8));
            }

            // ===================
            // END OF PLATFORM DEPENDENT CODE
            // ===================
            // 4. Put it into register with an extended sign

            ptr_rt->set(this->sign_extend(output));
        }
    }

    // Force template compilation:
    // LB => 
    template class MemoryReadWriteIInstruction<false, 1u, true>;
    // LBU => 
    template class MemoryReadWriteIInstruction<false, 1u, false>;
    // LH => 
    template class MemoryReadWriteIInstruction<false, 2u, true>;
    // LHU => 
    template class MemoryReadWriteIInstruction<false, 2u, false>;
    // LW
    template class MemoryReadWriteIInstruction<false, 4u, false>;
    // SB
    template class MemoryReadWriteIInstruction<true, 1u, false>;
    // SH
    template class MemoryReadWriteIInstruction<true, 2u, false>;
    // SW
    template class MemoryReadWriteIInstruction<true, 4u, false>;

    LoadIInstruction::LoadIInstruction(transform_t fnc_in) : fnc(fnc_in) {}

    void LWRIInstruction::execute() {
        uint32_t addr = get_addr();
        uint8_t buffer[4];
        uint32_t offset = (addr % 4),
                 read_addr = addr - offset;
        uint32_t and_mask, or_mask;
        or_mask =  0x00000000ul;
        and_mask = 0x00000000ul;

        // 1. Read from memory (throw exception if needed)
        WITH_CHECK_ERROR(
            mips_mem_read(
                ptr_mem, read_addr, 4, buffer 
            )
        );

        // 2. load into data
        // and create OR
        // transverse from LSB to MSB
        for (uint32_t i = 0 ; i < 1 + offset ; i++) {
            or_mask |= ((uint32_t(buffer[offset - i]) & 0xFF) << (8 * i));
        }

        for (uint32_t i = 0 ; i < 3 - offset ; i++) {
            and_mask |= ((0xFF) << (8 * (3 - i)));
        }

        ptr_rt->set((ptr_rt->get() & and_mask) | or_mask);
    }

    void LWLIInstruction::execute() {
        uint32_t addr = get_addr();
        uint8_t buffer[4];
        uint32_t offset = (addr % 4),
                 read_addr = addr - offset;
        uint32_t and_mask, or_mask;
        or_mask =  0x00000000ul;

        // 1. Read from memory (throw exception if needed)
        WITH_CHECK_ERROR(
            mips_mem_read(
                ptr_mem, read_addr, 4, buffer 
            )
        );

        // 2. load into data
        // and create OR
        for (uint32_t i = 0 ; i < 1 + (3 - offset) ; i++) {
            or_mask |= (uint32_t(buffer[offset + i]) & 0xFF) << (32 - 8 * (i + 1));
        }

        and_mask = 0x00000000ul;
        for (uint32_t i = 0 ; i < offset ; i++) {
            and_mask |= ((0xFF) << (8 * i));
        }

        ptr_rt->set((ptr_rt->get() & and_mask) | or_mask);
    }

    void LoadIInstruction::execute() {
        ptr_rt->set(fnc(konst));
    }

    BranchIInstruction::BranchIInstruction(
        comparator_t fnc_in,
        std::queue<std::pair<uint32_t, MIPS::jump_t>> * ptr_delayed_branches_in
    ) {
        fnc = fnc_in;
        ptr_delayed_branches = ptr_delayed_branches_in;
    }

    void BranchIInstruction::execute() {
        if (fnc(ptr_rs->get(), ptr_rt->get())) {
            // note : relative branching of the NEXT instruction
            ptr_delayed_branches->push(
                std::make_pair(
                    functions::sign_extend<16>(konst) << 2,
                    MIPS::J_BRANCHES
                )
            );
        }
    }

    void BranchLinkIInstruction::set_pc(RegisterInterface * ptr_pc_in) {
        ptr_pc = ptr_pc_in;
    }

    void BranchLinkIInstruction::set_r_return(RegisterInterface * ptr_r_return_in) {
        ptr_r_return = ptr_r_return_in;
    }

    void BranchLinkIInstruction::execute() {
        BranchIInstruction::execute();
        // Branch link should modify the stack pointer no matter what right?
        ptr_r_return->set(ptr_pc->get() + 8);
    }

    void JInstruction::set_addr(uint32_t arg) {
        addr = arg;
    }

    JumpJInstruction::JumpJInstruction(
        std::queue<std::pair<uint32_t, MIPS::jump_t>> * ptr_delayed_branches_in
    ) {
        ptr_delayed_branches = ptr_delayed_branches_in;
    }

    void JumpJInstruction::execute() {
        ptr_delayed_branches->push(
            std::make_pair(
                addr << 2,
                MIPS::J_J
            )
        );
    }

    void JumpLinkJInstruction::set_ptr_r_return(
        RegisterInterface* ptr_r_return_in
    ) {
        ptr_r_return = ptr_r_return_in;
    }

    void JumpLinkJInstruction::set_ptr_pc(
        RegisterInterface* ptr_pc_in
    ) {
        ptr_pc = ptr_pc_in;
    }

    void JumpLinkJInstruction::execute() {
        JumpJInstruction::execute();
        ptr_r_return->set(ptr_pc->get() + 8);
    }
}

