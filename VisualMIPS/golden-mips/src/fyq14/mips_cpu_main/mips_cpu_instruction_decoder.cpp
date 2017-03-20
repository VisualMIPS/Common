#include "MIPS/instruction_decoder.h"
#include "MIPS/cpu.h"

#define ENSURE(cond) \
    if (!(cond)) { \
        throw InvalidInstructionException(); \
    } \

#define DEBUG(...) \
    ptr_cpu->logger.debug(__VA_ARGS__)

namespace MIPS {
    namespace bitsconstants {
        // RInsttructions
        const uint32_t ADD = 0b100000;
        const uint32_t ADDU = 0b100001;
        const uint32_t AND = 0b100100;
        const uint32_t OR = 0b100101;
        const uint32_t SUB = 0b100010;
        const uint32_t SUBU = 0b100011;
        const uint32_t XOR = 0b100110;
        const uint32_t DIV = 0b011010;
        const uint32_t DIVU = 0b011011;
        const uint32_t MULT = 0b011000;
        const uint32_t MULTU = 0b011001;
        const uint32_t MTLO = 0b010011;
        const uint32_t MTHI = 0b010001;
        const uint32_t MFLO = 0b010010;
        const uint32_t MFHI = 0b010000;
        const uint32_t SLT = 0b101010;
        const uint32_t SLTU = 0b101011;
        const uint32_t SLL = 0b000000;
        const uint32_t SLLV = 0b000100;
        const uint32_t SRA = 0b000011;
        const uint32_t SRAV = 0b000111;
        const uint32_t SRL = 0b000010;
        const uint32_t SRLV = 0b000110;
        const uint32_t JR = 0b001000;
        const uint32_t JALR = 0b001001;

        // IInstruction
        const uint32_t ADDI = 0b001000;
        const uint32_t ADDIU = 0b001001;
        const uint32_t ANDI = 0b001100;
        const uint32_t ORI = 0b001101;
        const uint32_t XORI = 0b001110;
        const uint32_t BEQ = 0b000100;
        const uint32_t BNE = 0b000101;
        const uint32_t BLEZ = 0b000110;
        const uint32_t BGTZ = 0b000111;
        const uint32_t REGIMM = 0b000001;
        const uint32_t BGEZ = 0b00001;
        const uint32_t BLTZ = 0b00000;
        const uint32_t BGEZAL = 0b10001;
        const uint32_t BLTZAL = 0b10000;
        const uint32_t LB = 0b100000;
        const uint32_t LBU = 0b100100;
        const uint32_t LH = 0b100001;
        const uint32_t LHU = 0b100101;
        const uint32_t LW = 0b100011;
        const uint32_t LUI = 0b001111;
        const uint32_t SB = 0b101000;
        const uint32_t SW = 0b101011;
        const uint32_t SH = 0b101001;
        const uint32_t SLTIU = 0b001011;
        const uint32_t SLTI = 0b001010;
        const uint32_t LWL = 0b100010;
        const uint32_t LWR = 0b100110;
        
        // JInstructions
        const uint32_t JAL = 0b000011;
        const uint32_t J = 0b000010;
    }

    InstructionDecoder::InstructionDecoder(mips_cpu_impl & cpu) {
        ptr_cpu = &cpu;
    }

    Instruction * InstructionDecoder::decode(
        instruction_t bits
    ) {
        // The logic for decoding the bits (denotes as bits) should occur
        // here, and the output should be assigned to x
        //
        // check the first six bits
        // 1. if it is 0b000000 it is R
        // 2. if it is anything for I it is I
        // 3. if it is anything for J it is J
        // the logic for part 2 and 3 is carried out by decodeIJInstruction
        DEBUG("    Decoding 0x%.8X\n", bits);
        try {
            switch ((bits >> 26) & 0x3F) {
                case 0x000000:
                    return decodeRInstruction(bits);
                default:
                    return decodeIJInstruction(bits);
            }
        } catch (MIPS::Exception e) {
            if (e.get_error_code() == mips_ExceptionInvalidInstruction) {
                return new InvalidInstruction();
            }
            throw e;
        }
    }

    Instruction * InstructionDecoder::decodeIJInstruction(instruction_t bits) {
        Instruction * instruction;
        try {
            instruction = decodeIInstruction(bits);
            return instruction;

        } catch (MIPS::Exception e) {
            if (e.get_error_code() == mips_ExceptionInvalidInstruction) {
                return decodeJInstruction(bits);
            }
            throw e;
        }
    }

    Instruction * InstructionDecoder::decodeJInstruction(
        instruction_t bits
    ) {
        uint32_t op = (bits >> 26) & 0x3F,
                 addr = bits & 0x3FFFFFF;
        JInstruction * instruction;

        switch (op) {
        case bitsconstants::JAL: {
            JumpLinkJInstruction * ptr =
                new JumpLinkJInstruction(&ptr_cpu->delayed_branches);
            ptr->set_ptr_r_return(ptr_cpu->ptr_registers[31]);
            ptr->set_ptr_pc(ptr_cpu->ptr_pc);
            instruction = static_cast<JInstruction*>(ptr);
            DEBUG("    => DECODED! instruction_class = JumpLinkJInstruction\n");
            break;
        }

        case bitsconstants::J:
            instruction = new JumpJInstruction(&ptr_cpu->delayed_branches);
            DEBUG("    => DECODED! instruction_class = JumpJInstruction\n");
            break;

        default:
            throw InvalidInstructionException();
        }
        instruction->set_addr(addr);

        return instruction;
    }

    bool InstructionDecoder::can_rinstruction_overflow(
        instruction_t bits
    ) {
        switch (bits & 0x3F) {
            case bitsconstants::OR:
            case bitsconstants::AND:
            case bitsconstants::XOR:
            case bitsconstants::ADDU:
            case bitsconstants::SUBU:
            case bitsconstants::SLTU:
            case bitsconstants::SLT:
                return false;

            case bitsconstants::SUB:
            case bitsconstants::ADD:
                return true;
        }

        throw InvalidInstructionException();
    }

    operator_t InstructionDecoder::get_rinstruction_fnc(
        instruction_t bits
    ) {
        switch (bits & 0x3F) {
            case bitsconstants::ADDU:
            case bitsconstants::ADD:
                return MIPS::functions::addition;

            case bitsconstants::SUB:
            case bitsconstants::SUBU:
                return MIPS::functions::subtraction;

            case bitsconstants::AND:
                return MIPS::functions::bitwise_and;

            case bitsconstants::OR:
                return MIPS::functions::bitwise_or;

            case bitsconstants::XOR:
                return MIPS::functions::bitwise_xor;

            case bitsconstants::SLTU:
                return MIPS::functions::slt<false>;

            case bitsconstants::SLT:
                return MIPS::functions::slt<true>;
        }
        return NULL;
    }

    operator_t InstructionDecoder::get_rinstruction_lo_fnc(instruction_t bits) {
        switch(bits & 0x3F) {
        case bitsconstants::DIV:
            return functions::division<true>;
        case bitsconstants::DIVU:
            return functions::division<false>;
        case bitsconstants::MULTU:
            return functions::multiplication_lo<false>;
        case bitsconstants::MULT:
            return functions::multiplication_lo<true>;
        }

        throw InvalidInstructionException();
    }

    operator_t InstructionDecoder::get_rinstruction_hi_fnc(instruction_t bits) {
        switch(bits & 0x3F) {
        case bitsconstants::DIV:
            return functions::modulo<true>;
        case bitsconstants::DIVU:
            return functions::modulo<false>;
        case bitsconstants::MULTU:
            return functions::multiplication_hi<false>;
        case bitsconstants::MULT:
            return functions::multiplication_hi<true>;
        }

        throw InvalidInstructionException();
    }

    operator_t InstructionDecoder::get_iinstruction_fnc(instruction_t bits) {
        switch ((bits >> 26) & 0b111111) {
            case bitsconstants::ADDI:
            case bitsconstants::ADDIU:
                return functions::addition;

            case bitsconstants::ANDI:
                return functions::bitwise_and;

            case bitsconstants::ORI:
                return functions::bitwise_or;

            case bitsconstants::XORI:
                return functions::bitwise_xor;

            case bitsconstants::SLTIU:
                return functions::slt<false>;

            case bitsconstants::SLTI:
                return functions::slt<true>;
        }

        throw InvalidInstructionException();
    }

    bool InstructionDecoder::can_iinstruction_overflow(instruction_t bits) {
        switch ((bits >> 26) & 0b111111) {
            case bitsconstants::ADDI:
                return true;
        }
        return false;
    }

    IInstruction * InstructionDecoder::make_loader_storer(instruction_t bits) {
        mips_mem_h mem = ptr_cpu->mem_handler;
        switch ((bits >> 26) & 0x3F) {

        case bitsconstants::SB:
            return new MemoryReadWriteIInstruction<true, 1, false>(mem);

        case bitsconstants::SH:
            return new MemoryReadWriteIInstruction<true, 2, false>(mem);

        case bitsconstants::SW:
            return new MemoryReadWriteIInstruction<true, 4, false>(mem);

        case bitsconstants::LB:
            return new MemoryReadWriteIInstruction<false, 1, true>(mem);

        case bitsconstants::LBU:
            return new MemoryReadWriteIInstruction<false, 1, false>(mem);

        case bitsconstants::LH:
            return new MemoryReadWriteIInstruction<false, 2, true>(mem);

        case bitsconstants::LHU:
            return new MemoryReadWriteIInstruction<false, 2, false>(mem);

        case bitsconstants::LW:
            return new MemoryReadWriteIInstruction<false, 4, false>(mem);

        }

        throw InvalidInstructionException();
    }

    Instruction * InstructionDecoder::decodeIInstruction(
        instruction_t bits
    ) {
        uint32_t rs_idx = (bits >> 21) & 0b11111,
                 rt_idx = (bits >> 16) & 0b11111,
                 konst = bits & 0xFFFF;

        switch ((bits >> 26) & 0b111111) {
            case bitsconstants::SLTI:
            case bitsconstants::SLTIU:
            case bitsconstants::ADDI:
            case bitsconstants::ADDIU: {

                operator_t fnc = get_iinstruction_fnc(bits);
                ArithmeticIInstruction * instruction =
                    new ArithmeticIInstruction(fnc, can_iinstruction_overflow(bits));
                instruction->set_rt(ptr_cpu->ptr_registers[rt_idx]);
                instruction->set_rs(ptr_cpu->ptr_registers[rs_idx]);
                instruction->set_konst(konst);
                DEBUG(
                    "    => DECODE! instruction_class = ArithmeticIInstruction\n"
                );
                return instruction;
            }
            case bitsconstants::ANDI:
            case bitsconstants::ORI:
            case bitsconstants::XORI: {
                BitwiseIInstruction * instruction =
                    new BitwiseIInstruction(get_iinstruction_fnc(bits));
                instruction->set_rt(ptr_cpu->ptr_registers[rt_idx]);
                instruction->set_rs(ptr_cpu->ptr_registers[rs_idx]);
                instruction->set_konst(konst);
                DEBUG(
                    "    => DECODE! instruction_class = BitwiseIInstruction\n"
                );
                return instruction;
            }

            case bitsconstants::REGIMM: {
                switch (rt_idx) {
                case bitsconstants::BGEZ:
                case bitsconstants::BLTZ: {
                    BranchIInstruction * instruction =
                        new BranchIInstruction(
                            get_comparator(bits),
                            &ptr_cpu->delayed_branches
                        );
                    instruction->set_rs(ptr_cpu->ptr_registers[rs_idx]);
                    instruction->set_rt(ptr_cpu->ptr_registers[0]);
                    instruction->set_konst(konst);
                    DEBUG(
                        "    => DECODED! instruction_class = BranchIInstruction (REGIMM)\n"
                    );
                    return instruction;
                }

                case bitsconstants::BGEZAL:
                case bitsconstants::BLTZAL: {

                    BranchLinkIInstruction * instruction =
                        new BranchLinkIInstruction(
                            get_comparator(bits),
                            &ptr_cpu->delayed_branches
                        );

                    instruction->set_pc(ptr_cpu->ptr_pc);
                    instruction->set_rs(ptr_cpu->ptr_registers[rs_idx]);
                    instruction->set_rt(ptr_cpu->ptr_registers[0]);
                    instruction->set_r_return(ptr_cpu->ptr_registers[31]);
                    instruction->set_konst(konst);
                    DEBUG(
                        "    => DECODED! instruction_class = BranchLinkIInstruction\n"
                    );

                    return instruction;
                }

                default:
                    throw InvalidInstructionException();
                }
            }

            case bitsconstants::BGTZ:
            case bitsconstants::BLEZ: {
                ENSURE((rt_idx & 0x1F) == 0b00000);

                BranchIInstruction * instruction =
                    new BranchIInstruction(
                        get_comparator(bits),
                        &ptr_cpu->delayed_branches
                    );
                instruction->set_rs(ptr_cpu->ptr_registers[rs_idx]);
                instruction->set_rt(ptr_cpu->ptr_registers[0]);
                instruction->set_konst(konst);
                return instruction;
            }

            case bitsconstants::BNE:
            case bitsconstants::BEQ: {
                comparator_t fnc = get_comparator(bits);
                BranchIInstruction * instruction =
                    new BranchIInstruction(
                        fnc,
                        &ptr_cpu->delayed_branches
                    );
                instruction->set_rs(ptr_cpu->ptr_registers[rs_idx]);
                instruction->set_rt(ptr_cpu->ptr_registers[rt_idx]);
                instruction->set_konst(konst);

                DEBUG("    => DECODED! instruction_class = BranchIInstruction\n");
                return instruction;
            }

            case bitsconstants::SB:
            case bitsconstants::SH:
            case bitsconstants::SW:
            case bitsconstants::LHU:
            case bitsconstants::LH:
            case bitsconstants::LBU:
            case bitsconstants::LB:
            case bitsconstants::LW: {
                // LB is 16 bits => 2 bytes
                IInstruction * instruction = make_loader_storer(bits);

                instruction->set_rs(ptr_cpu->ptr_registers[rs_idx]);
                instruction->set_rt(ptr_cpu->ptr_registers[rt_idx]);
                instruction->set_konst(konst);

                DEBUG("    => DECODED! instruction_class = MemoryReadWriteIInstruction<>\n");

                return instruction;
            }

            case bitsconstants::LUI: {
                ENSURE(rs_idx == 0);
                IInstruction * instruction =
                    new LoadIInstruction(functions::left_shift<16>);
                // Defensive programming :D
                instruction->set_rs(ptr_cpu->ptr_registers[0]);
                instruction->set_rt(ptr_cpu->ptr_registers[rt_idx]);
                instruction->set_konst(konst);
                return instruction;
            }

            case bitsconstants::LWL: {
                IInstruction * instruction =
                    new LWLIInstruction(ptr_cpu->mem_handler);
                instruction->set_rs(ptr_cpu->ptr_registers[rs_idx]);
                instruction->set_rt(ptr_cpu->ptr_registers[rt_idx]);
                instruction->set_konst(konst);
                return instruction;
            }
            case bitsconstants::LWR: {
                IInstruction * instruction =
                    new LWRIInstruction(ptr_cpu->mem_handler);
                instruction->set_rs(ptr_cpu->ptr_registers[rs_idx]);
                instruction->set_rt(ptr_cpu->ptr_registers[rt_idx]);
                instruction->set_konst(konst);
                return instruction;
            }
            default:
                throw InvalidInstructionException();
        }

        throw InvalidInstructionException();
    }

    comparator_t InstructionDecoder::get_comparator(instruction_t bits) {
        switch ((bits >> 26) & 0x3F) {
        case bitsconstants::REGIMM: {
            switch ((bits >> 16) & 0x1F) {
            case bitsconstants::BLTZAL:
            case bitsconstants::BLTZ:
                return functions::lt;
            case bitsconstants::BGEZAL:
            case bitsconstants::BGEZ:
                return functions::gte;
            }
            break;
        }
        case bitsconstants::BEQ:
            return functions::eq;
        case bitsconstants::BNE:
            return functions::neq;
        case bitsconstants::BGTZ:
            return functions::gt;
        case bitsconstants::BLEZ:
            return functions::lte;
        }

        throw InvalidInstructionException();
    }

    Instruction * InstructionDecoder::make_mover(
        RegisterInterface * dest,
        RegisterInterface * src 
    ) {
        ArithmeticRInstruction * instruction =
            new ArithmeticRInstruction(functions::addition, false);
        instruction->set_rs(src);
        instruction->set_rt(ptr_cpu->ptr_registers[0]);
        instruction->set_rd(dest);
        return instruction;
    }

    shift_fnc_t InstructionDecoder::get_rinstruction_shift_fnc(
        instruction_t bits
    ) {
        switch(bits & 0x3F) {
            case bitsconstants::SLL:
            case bitsconstants::SLLV:
                return functions::left_shift;

            case bitsconstants::SRA:
            case bitsconstants::SRAV:
                return functions::right_shift<true>;

            case bitsconstants::SRL:
            case bitsconstants::SRLV:
                return functions::right_shift<false>;
        }

        throw InvalidInstructionException();
    }

    Instruction * InstructionDecoder::decodeRInstruction(
        instruction_t bits
    ) {
        uint32_t rs = (bits >> 21) & 0x1F,
                 rt = (bits >> 16) & 0x1F,
                 rd = (bits >> 11) & 0x1F,
                 shift = (bits >> 6) & 0x1F;
        switch (bits & 0x3F) {
        case bitsconstants::ADDU:
        case bitsconstants::ADD:
        case bitsconstants::SUB:
        case bitsconstants::SUBU:
        case bitsconstants::AND:
        case bitsconstants::OR:
        case bitsconstants::XOR:
        case bitsconstants::SLT:
        case bitsconstants::SLTU: {
            ENSURE(shift == 0);

            operator_t fnc = get_rinstruction_fnc(bits);
            bool can_overflow = can_rinstruction_overflow(bits);

            DEBUG(
                "    => DECODED! insturction_class = "
            );
            if (can_overflow) {
                DEBUG(
                    "ArithmeticRInstruction (signed)\n"
                );
            } else {
                DEBUG(
                    "ArithmeticRInstruction (unsigned)\n"
                );
            }

            ArithmeticRInstruction* instruction =
                new ArithmeticRInstruction(fnc, can_overflow);
            instruction->set_rs(ptr_cpu->ptr_registers[rs]);
            instruction->set_rt(ptr_cpu->ptr_registers[rt]);
            instruction->set_rd(ptr_cpu->ptr_registers[rd]);
            return instruction;
        }

        case bitsconstants::SRL:
        case bitsconstants::SRA:
        case bitsconstants::SLL:
            ENSURE(rs == 0);
            goto label_create_shift_object; // I promise, this is the only goto in my code besides, dinosaurs are extinct, so this probably won't happen (https://xkcd.com/292/) . But I really want to have a goto in my code, so it feels kinda cool . Don't worry - the label is just 5 lines before this statement

        case bitsconstants::SRLV:
        case bitsconstants::SLLV:
        case bitsconstants::SRAV: {

            ENSURE(shift == 0);

label_create_shift_object:
            shift_fnc_t fnc = get_rinstruction_shift_fnc(bits);
            ShiftRInstruction * instruction =
                new ShiftRInstruction(fnc);
            instruction->set_rd(ptr_cpu->ptr_registers[rd]);
            instruction->set_rs(ptr_cpu->ptr_registers[rs]);
            instruction->set_rt(ptr_cpu->ptr_registers[rt]);
            instruction->set_shift(shift);
            return instruction;
        }

        case bitsconstants::MULTU:
        case bitsconstants::MULT:
        case bitsconstants::DIV:
        case bitsconstants::DIVU: {
            ENSURE(rd == 0 && shift == 0);

            operator_t lo_fnc = get_rinstruction_lo_fnc(bits);
            operator_t hi_fnc = get_rinstruction_hi_fnc(bits);

            MultDivInstruction * instruction = 
                new MultDivInstruction(lo_fnc, hi_fnc);
            instruction->set_r_lo(ptr_cpu->ptr_register_lo);
            instruction->set_r_hi(ptr_cpu->ptr_register_hi);
            instruction->set_rs(ptr_cpu->ptr_registers[rs]);
            instruction->set_rt(ptr_cpu->ptr_registers[rt]);
            
            DEBUG("    => DECODED! instruction_class = ");
            DEBUG("MultDivInstruction\n");

            return instruction;
        }

        case bitsconstants::MFHI:
            ENSURE(rs == 0 && rt == 0 && shift == 0);
            return make_mover(ptr_cpu->ptr_registers[rd], ptr_cpu->ptr_register_hi);
        case bitsconstants::MTHI:
            ENSURE(rd == 0 && rt == 0 && shift == 0);
            return make_mover(ptr_cpu->ptr_register_hi, ptr_cpu->ptr_registers[rs]);
        case bitsconstants::MFLO:
            ENSURE(rs == 0 && rt == 0 && shift == 0);
            return make_mover(ptr_cpu->ptr_registers[rd], ptr_cpu->ptr_register_lo);
        case bitsconstants::MTLO:
            ENSURE(rd == 0 && rt == 0 && shift == 0);
            return make_mover(ptr_cpu->ptr_register_lo, ptr_cpu->ptr_registers[rs]);

        case bitsconstants::JR: {
            ENSURE(rd == 0 && shift == 0 && rt == 0);
            JumpRInstruction * instruction =
                new JumpRInstruction(&ptr_cpu->delayed_branches);
            instruction->set_rs(ptr_cpu->ptr_registers[rs]);
            return instruction;
        }

        case bitsconstants::JALR: {
            ENSURE(rt == 0 && shift == 0);
            JumpLinkRInstruction * instruction =
                new JumpLinkRInstruction(&ptr_cpu->delayed_branches);
            instruction->set_pc(ptr_cpu->ptr_pc);
            instruction->set_rs(ptr_cpu->ptr_registers[rs]);
            instruction->set_rd(ptr_cpu->ptr_registers[rd]);
            return instruction;
        }

        }

        DEBUG(
            "    => FAILED DECODING in decodeRInstruction! "
        );
        DEBUG(
            "Instruction Class = InvalidInstruction\n"
        );
        throw InvalidInstructionException();
    }

    void InstructionDecoder::clear() {
        while(!instruction_queue.empty()) {
            Instruction * instruction = instruction_queue.front();
            instruction_queue.pop();
            delete instruction;
        }
    }

    InstructionDecoder& operator>>(
        InstructionDecoder & decoder,
        Instruction * & ptr_instruction
    ) {
        ptr_instruction = decoder.instruction_queue.front();
        decoder.instruction_queue.pop();
        return decoder;
    }

    InstructionDecoder& operator<<(
        InstructionDecoder & decoder,
        instruction_t instruction_vector
    ) {
        decoder.instruction_queue.push(
            decoder.decode(instruction_vector)
        );
        return decoder;
    }
}

