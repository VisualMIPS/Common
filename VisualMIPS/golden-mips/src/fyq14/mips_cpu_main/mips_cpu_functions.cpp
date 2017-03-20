#include "MIPS/functions.h"
#include <iostream>

namespace MIPS {
    namespace functions {
        namespace helpers {
            template <typename T>
            static T smart_cast(uint32_t a) {
                T x;
                if (std::is_signed<T>()) {
                    x = static_cast<T>(int32_t(a));
                } else {
                    x =  static_cast<T>(a);
                }

                return x;
            }

            template <typename T>
            static bool modulo(uint32_t a, uint32_t b, uint32_t * res) {
                T x, y, z;
                x = smart_cast<T>(a);
                y = smart_cast<T>(b);

                if (y != 0) {
                    z = x % y;
                    *res = static_cast<uint32_t>(z);
                }
                return false;
            }

            template <typename T>
            static bool division(uint32_t a, uint32_t b, uint32_t * res) {
                T x, y, z;
                x = smart_cast<T>(a);
                y = smart_cast<T>(b);

                if (y != 0) {
                    z = x / y;
                    *res = static_cast<uint32_t>(z);
                }
                return false;
            }

            template<typename T>
            static bool multiplication_hi(uint32_t a, uint32_t b, uint32_t * res) {
                T x, y, z;
                x = smart_cast<T>(a);
                y = smart_cast<T>(b);
                z = (x * y) >> 32;

                *res = static_cast<uint32_t>(z);

                return false;
            }

            template<typename T>
            static bool multiplication_lo(uint32_t a, uint32_t b, uint32_t * res) {
                T x, y, z;
                x = smart_cast<T>(a);
                y = smart_cast<T>(b);

                z = (x * y) & 0xFFFFFFFF;

                *res = static_cast<uint32_t>(z);

                return false;
            }

            static bool adder_check_overflow(uint32_t a, uint32_t b, uint32_t c_i) {
                uint32_t b_i, a_i; 
                uint32_t c_o;
                a_i = (a & 0x1);
                b_i = (b & 0x1);
                uint32_t c = (a_i & b_i) | ((a_i ^ b_i) & c_i);

                for (uint32_t i = 1 ; i < 31 ; i++) {
                    a_i = (b >> i) & 0x1;
                    b_i = (a >> i) & 0x1;
                    c = (b_i & c) | (a_i & b_i) | (c & a_i);
                }
                // c now contains c_in31
                a_i = (a >> 31) & 0x1;
                b_i = (b >> 31) & 0x1;
                c_o = (b_i & c) | (a_i & b_i) | (c & a_i);


                return ((c) ^ (c_o));
            }
        }

        bool addition(uint32_t a, uint32_t b, uint32_t * res) {
            *res = a + b;
            return helpers::adder_check_overflow(a, b, 0);
        }

        bool subtraction (uint32_t a, uint32_t b, uint32_t* res) {
            *res = a - b;
            return helpers::adder_check_overflow(a, ~b, 1);
        }

        bool bitwise_and (uint32_t a, uint32_t b, uint32_t* res) {
            *res = a & b;
            return false;
        }

        bool bitwise_or (uint32_t a, uint32_t b, uint32_t* res) {
            *res = a | b;
            return false;
        }

        bool bitwise_xor (uint32_t a, uint32_t b, uint32_t* res) {
            *res = a ^ b;
            return false;
        }

        template<bool is_signed>
        bool division(uint32_t a, uint32_t b, uint32_t* res) {
            if (is_signed) {
                return helpers::division<int64_t>(a, b, res);
            } else {
                return helpers::division<uint64_t>(a, b, res);
            }
        }

        template bool division<true>(uint32_t, uint32_t, uint32_t*);
        template bool division<false>(uint32_t, uint32_t, uint32_t*);

        template<bool is_signed>
        bool modulo(uint32_t a, uint32_t b, uint32_t* res) {
            if (is_signed) {
                return helpers::modulo<int64_t>(a, b, res);
            } else {
                return helpers::modulo<uint64_t>(a, b, res);
            }
        }

        template bool modulo<true>(uint32_t, uint32_t, uint32_t*);
        template bool modulo<false>(uint32_t, uint32_t, uint32_t*);

        template<bool is_signed>
        bool multiplication_lo(uint32_t a, uint32_t b, uint32_t* res) {
            if (is_signed) {
                return helpers::multiplication_lo<int64_t>(a, b, res);
            } else {
                return helpers::multiplication_lo<uint64_t>(a, b, res);
            }
        }

        template bool multiplication_lo<true>(uint32_t, uint32_t, uint32_t*);
        template bool multiplication_lo<false>(uint32_t, uint32_t, uint32_t*);
    
        template<bool is_signed>
        bool multiplication_hi(uint32_t a, uint32_t b, uint32_t* res) {
            if (is_signed) {
                return helpers::multiplication_hi<int64_t>(a, b, res);
            } else {
                return helpers::multiplication_hi<uint64_t>(a, b, res);
            }
        }

        template bool multiplication_hi<true>(uint32_t, uint32_t, uint32_t*);
        template bool multiplication_hi<false>(uint32_t, uint32_t, uint32_t*);

        bool eq(uint32_t a, uint32_t b) {
            return a == b;
        }

        bool neq(uint32_t a, uint32_t b) {
            return a != b;
        }

        bool gt(uint32_t a, uint32_t b) {
            return int32_t(a) > int32_t(b);
        }

        bool gte(uint32_t a, uint32_t b) {
            return int32_t(a) >= int32_t(b);
        }

        bool lt(uint32_t a, uint32_t b) {
            return int32_t(a) < int32_t(b);
        }

        bool lte(uint32_t a, uint32_t b) {
            return int32_t(a) <= int32_t(b);
        }

        uint32_t sign_extend(uint32_t a, uint32_t n) {
            // n refers to the number of bits
            // create a bit mask (expensive (well, who cares anyway ...)
            // but done at compile time

            uint32_t sign_bits = 0;
            for (uint32_t i = 31 ; i >= n ; i--) {
                sign_bits = sign_bits | (1 << i); 
            }

            if ((a >> (n-1)) & 0b1) {
                // Negative number, extend the sign
                return a | sign_bits;
            } else {
                // Positive number
                return a;
            }
        }

        template <bool is_signed>
        bool slt(uint32_t a, uint32_t b, uint32_t * res) {
            if (is_signed) {
                int32_t x, y;
                x = int32_t(a), y = int32_t(b);
                if (x < y) {
                    *res = 0x00000001l;
                } else {
                    *res = 0x00000000l;
                }
            } else {
                if (a < b) {
                    *res = 0x00000001l;
                } else {
                    *res = 0x00000000l;
                }
            }
            return false;
        }

        template bool slt<true>(uint32_t, uint32_t, uint32_t*);
        template bool slt<false>(uint32_t, uint32_t, uint32_t*);

        template<bool is_arithmetic>
        uint32_t right_shift(uint32_t arg, uint32_t n) {

            if (is_arithmetic) {
                return sign_extend(arg >> n, 32 - n);
            } else {
                return (arg >> n);
            }
        }

        template uint32_t right_shift<true>(uint32_t, uint32_t);
        template uint32_t right_shift<false>(uint32_t, uint32_t);

        uint32_t left_shift(uint32_t arg, uint32_t n) {
            return (arg << n);
        }
    }
}
