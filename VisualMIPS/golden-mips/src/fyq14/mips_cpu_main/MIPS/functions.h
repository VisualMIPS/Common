#ifndef MIPS_FUNCTIONS_H
#define MIPS_FUNCTIONS_H

#include <iostream>
using namespace std;
#include "mips_core.h"

namespace MIPS {
    namespace functions {
        // Simple Arithmetic
        bool addition (uint32_t, uint32_t, uint32_t*);
        bool subtraction (uint32_t, uint32_t, uint32_t*);

        // Multidv (a bit of mess, but following the code base convention to have signed/ unsigned)
        // stuff specified as template args
        template<bool is_signed>
        bool multiplication_lo(uint32_t, uint32_t, uint32_t*);
        template<bool is_signed>
        bool multiplication_hi(uint32_t, uint32_t, uint32_t*);
        template<bool is_signed>
        bool division(uint32_t, uint32_t, uint32_t*);
        template<bool is_signed>
        bool modulo(uint32_t, uint32_t, uint32_t*);

        // Bitwise stuff
        bool bitwise_and (uint32_t, uint32_t, uint32_t*);
        bool bitwise_or (uint32_t, uint32_t, uint32_t*);
        bool bitwise_xor (uint32_t, uint32_t, uint32_t*);
        bool eq(uint32_t, uint32_t);
        bool neq(uint32_t, uint32_t);
        bool gt(uint32_t, uint32_t);
        bool gte(uint32_t, uint32_t);
        bool lt(uint32_t, uint32_t);
        bool lte(uint32_t, uint32_t);

        uint32_t sign_extend(uint32_t a, uint32_t n);

        template<bool is_arithmetic>
        uint32_t right_shift(uint32_t arg, uint32_t n);
        uint32_t left_shift(uint32_t arg, uint32_t n);

        // SLTs
        template<bool is_signed>
        bool slt(uint32_t, uint32_t, uint32_t *);

        template<unsigned n>
        uint32_t sign_extend(uint32_t a) {
            return sign_extend(a, n);
        }

        template<unsigned n>
        uint32_t left_shift(uint32_t arg) {
            return (arg << n);
        }
    }
}

#endif
