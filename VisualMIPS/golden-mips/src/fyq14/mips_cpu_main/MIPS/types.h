#ifndef MIPS_TYPES_H
#define MIPS_TYPES_H

#include "mips.h"

// Types
typedef uint32_t data_t;
typedef uint32_t addr_t;
typedef uint8_t mem_data_t;
typedef uint32_t instruction_t;
typedef bool (*operator_t)(uint32_t x, uint32_t y, uint32_t * res);
typedef bool (*comparator_t)(uint32_t x, uint32_t y);
typedef uint32_t (*transform_t) (uint32_t a);
typedef uint32_t (*shift_fnc_t) (uint32_t a, uint32_t b);

// Classes
namespace MIPS {
    class InstructionDecoder;
    class InstructionFetcher;
    class Instruction;

    enum jump_t { J_REG, J_BRANCHES, J_J };
}

typedef unsigned debug_level_t;

#endif
