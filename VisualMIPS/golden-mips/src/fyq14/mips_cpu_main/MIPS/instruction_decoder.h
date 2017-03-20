#ifndef MIPS_INSTRUCTION_DECODER_H
#define MIPS_INSTRUCTION_DECODER_H

#include <queue>
#include <functional>

#include "types.h"
#include "functions.h"
#include "instruction.h"

namespace MIPS {
    class InstructionDecoder {
        private:
            mips_cpu_impl * ptr_cpu;
            std::queue<Instruction*> instruction_queue;
            void decode(instruction_t, Instruction *);
            Instruction * decode(instruction_t);
            Instruction * decodeRInstruction(instruction_t);
            Instruction * make_mover(RegisterInterface *, RegisterInterface *);
            IInstruction * make_loader_storer(instruction_t bits);
            Instruction * decodeIJInstruction(instruction_t);
            Instruction * decodeIInstruction(instruction_t);
            Instruction * decodeJInstruction(instruction_t);
            bool can_rinstruction_overflow(instruction_t);
            bool can_iinstruction_overflow(instruction_t);
            operator_t get_rinstruction_fnc(instruction_t);
            operator_t get_rinstruction_lo_fnc(instruction_t);
            operator_t get_rinstruction_hi_fnc(instruction_t);
            operator_t get_iinstruction_fnc(instruction_t);
            shift_fnc_t get_rinstruction_shift_fnc(instruction_t);
            comparator_t get_comparator(instruction_t);
        public:
            InstructionDecoder(mips_cpu_impl&);
            void clear();
            friend InstructionDecoder& operator>>(
                InstructionDecoder &,
                Instruction * &
            );
            friend InstructionDecoder& operator<<(
                InstructionDecoder &,
                instruction_t
            );

    };
}

#endif
