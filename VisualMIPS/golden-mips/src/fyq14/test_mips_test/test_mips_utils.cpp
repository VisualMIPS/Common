#include "utils.h"
#include <sstream>

namespace test {

    void check_error(mips_error err) {
        if (err) {
            throw test::FailedTestException();
        }
    }

    void assert(bool flag) {
        if (!flag) {
            throw test::FailedTestException();
        }
    }

    void assert(bool flag, const std::string & reason){
        if (!flag) {
            throw test::FailedTestException();
        }
    }

    namespace utils {

        uint32_t fetch_integer(std::istream & fin) {
            std::string s;
            fin >> s;
            return from_string<uint32_t>(s);
        }

        std::string make_file_name(const char * instruction) {
            const char PREFIX[] = "test_mips_test/data/";
            const char POSTFIX[] = ".txt";
            unsigned length = strlen(PREFIX) + strlen(instruction) +
                strlen(POSTFIX) + 1;
            char * buffer = (char*) calloc(length, sizeof(char));

            strcpy(buffer, PREFIX);
            strcat(buffer, instruction);
            strcat(buffer, POSTFIX);

            std::string s_tmp(buffer);
            free(buffer);

            return s_tmp;
        }

        std::string make_file_name(const std::string & s_in) {
            return make_file_name(s_in.c_str());
        }

        void parse_arithmetic_input(std::string s, bool * is_overflow, uint32_t * results) {
            if (s == "OVERFLOW") {
                *is_overflow = true;
            } else {
                *is_overflow = false;
                *results = from_string<uint32_t>(s);
            }
        }

        void parse_multdiv_input(std::string s, bool * is_ok_anything, uint32_t * results) {
            if (s == "UNDEFINED") {
                *is_ok_anything = true;
            } else {
                *is_ok_anything = *is_ok_anything || false;
                *results = from_string<uint32_t>(s);
            }
        }

        uint32_t sign_extend_16(uint16_t a) {
            return uint32_t(int32_t(int16_t(a)));
        }

        namespace memory {

        mips_error write(mips_mem_h mem, uint32_t addr, uint32_t length, uint32_t * data) {
            uint8_t * buffer = new uint8_t[length * 4];
            for (uint32_t i = 0 ; i < length ; i++) {
                buffer[i*4 + 0] = (data[i] >> 24) & 0xFF;
                buffer[i*4 + 1] = (data[i] >> 16) & 0xFF;
                buffer[i*4 + 2] = (data[i] >> 8) & 0xFF;
                buffer[i*4 + 3] = (data[i] >> 0) & 0xFF;
            }
            mips_error err = mips_mem_write(mem, addr, 4 * length, buffer);
            delete[] buffer;
            return err;
        }

        }
    }
}
