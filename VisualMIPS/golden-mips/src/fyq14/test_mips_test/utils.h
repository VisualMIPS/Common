#ifndef TEST_UTILS_H
#define TEST_UTILS_H

#include <functional>
#include <algorithm>
#include <string>
#include <cstring>
#include <sstream>
#include <iostream>
#include <fstream>
#include <map>

#include "exception.h"
#include "mips_test.h"

#define TEST_CHECK_FILE_OPEN(fin, name) \
    if (!fin.is_open()) { \
        std::cerr << "Cannot find metadata file for instruction" << name << std::endl; \
        std::cerr << "whose file path was " << \
            test::utils::make_file_name(name) << std::endl; \
        std::cerr << "TERMINATING NOW" << std::endl; \
        exit(1); \
    }

typedef bool (*comparator_t)(uint32_t x, uint32_t y);
namespace test {

    void check_error(mips_error);
    void assert(bool);
    void assert(bool, const std::string &);

    namespace utils {

    template<typename T>
    T from_string(const std::string & arg) {
        std::istringstream ss(arg);
        T ret;

        if (arg.length() >= 2 &&
                arg[0] == '0' &&
                arg[1] == 'x') {
            ss >> std::hex >> ret;
            return ret;
        } else if (arg.length() >= 1 && arg[0] == '-') {
            int64_t tmp;
            ss >> tmp;
            return uint32_t(uint64_t(tmp) & 0xFFFFFFFF);
        }

        // else condition, in awkward position to make clang happy
        ss >> ret;
        return ret;
    }

    uint32_t fetch_integer(std::istream &);
    std::string make_file_name(const char *);
    std::string make_file_name(const std::string &);
    void parse_arithmetic_input(std::string, bool *, uint32_t *);
    uint32_t sign_extend_16(uint16_t);
    void parse_multdiv_input(std::string, bool *, uint32_t *);

        namespace memory {

        mips_error write(mips_mem_h, uint32_t, uint32_t, uint32_t *);

        }

    }
}

#endif
