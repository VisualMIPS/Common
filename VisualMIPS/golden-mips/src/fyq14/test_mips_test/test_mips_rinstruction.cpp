#include "test.h"
#include <iostream>
#include <string>
#include <fstream>
#include <vector>
#include <map>
using namespace std;

static map<string, uint32_t> m_constants;
static void init();
static void test_arithmetic(const string &);
static void test_mov_hi_lo(const string &);
static void test_multdiv(const string &);
static void test_jump(const string &);

// ===================================
// main section
// ===================================

void test_rinstructions() {
    init();
    // Arithmetic (various)
    test_arithmetic("SLL");
    test_arithmetic("SRL");
    test_arithmetic("SRA");
    test_arithmetic("SRAV");
    test_arithmetic("SRLV");
    test_arithmetic("SLLV");
    test_arithmetic("ADD");
    test_arithmetic("SUB");
    test_arithmetic("ADDU");
    test_arithmetic("SUBU");
    test_arithmetic("XOR");
    test_arithmetic("AND");
    test_arithmetic("OR");
    test_arithmetic("SLT");
    test_arithmetic("SLTU");

    // HI/LO Interatction
    test_mov_hi_lo("HI");
    test_mov_hi_lo("LO");

    // Multdiv
    test_multdiv("MULTU");
    test_multdiv("DIVU");
    test_multdiv("MULT");
    test_multdiv("DIV");

    // Jumps
    test_jump("JR");
    test_jump("JALR");
}

static void init() {
    static bool init_ = false;
    if (init_) {
        return;
    }
    m_constants["SPECIAL"] = 0b000000;
    m_constants["ADD"] = 0b100000;
    m_constants["SUB"] = 0b100010;
    m_constants["ADDU"] = 0b100001;
    m_constants["SUBU"] = 0b100011;
    m_constants["XOR"] = 0b100110;
    m_constants["OR"] = 0b100101;
    m_constants["AND"] = 0b100100;
    m_constants["MULTU"] = 0b011001;
    m_constants["DIVU"] = 0b011011;
    m_constants["MFLO"] = 0b010010;
    m_constants["MFHI"] = 0b010000;
    m_constants["MULT"] = 0b011000;
    m_constants["DIV"] = 0b011010;
    m_constants["SLL"] = 0b000000;
    m_constants["SLLV"] = 0b000100;
    m_constants["SRA"] = 0b000011;
    m_constants["SRAV"] = 0b000111;
    m_constants["SRL"] = 0b000010;
    m_constants["SRLV"] = 0b000110;
    m_constants["SLT"] = 0b101010;
    m_constants["SLTU"] = 0b101011;
    m_constants["JR"] = 0b001000;
    m_constants["JALR"] = 0b001001;
    m_constants["MFLO"] = 0b010010;
    m_constants["MFHI"] = 0b010000;
    m_constants["MTHI"] = 0b010001;
    m_constants["MTLO"] = 0b010011;

    init_ = true;
}

static uint32_t construct_instruction(
    const char * name,
    uint32_t rs,
    uint32_t rt,
    uint32_t rd,
    uint32_t shift
) {
    return ((m_constants["SPECIAL"] << 26) |
        (rs << 21) |
        (rt << 16) |
        (rd << 11) |
        ((0x1F & shift) << 6) |
        m_constants[name]);
}

static uint32_t construct_instruction(
    const string & name,
    uint32_t rs,
    uint32_t rt,
    uint32_t rd,
    uint32_t shift
) {
    return construct_instruction(name.c_str(), rs, rt, rd, shift);
}


static void test_mov_hi_lo(const string & name) {
    uint32_t start_addr = 0x120;
    bool is_successful = 0;
    string name_from, name_to;
    mips_mem_h mem = mips_mem_create_ram(0x20000, 4);
    mips_cpu_h cpu = mips_cpu_create(mem);
    ifstream fin(test::utils::make_file_name("HI_LO"));

    if (name == "HI") {
        name_from = "MFHI";
        name_to = "MTHI";
    } else if (name == "LO") {
        name_from = "MFLO";
        name_to = "MTLO";
    } else {
        cerr << "BOGUS TEST INPUT" << endl;
        exit(1);
    }

    while(1) {
        int test_id;
        uint32_t rs, rs_value,
                 rd, expected_output,
                 actual_output;
        uint32_t instructions[2];

        try {
            rs = test::utils::fetch_integer(fin); 
            rs_value = test::utils::fetch_integer(fin); 
            rd = test::utils::fetch_integer(fin);
            expected_output = test::utils::fetch_integer(fin);

            if(fin.eof()) { break; }

            instructions[0] = construct_instruction(
                name_to, rs, 0, 0, 0
            );
            instructions[1] = construct_instruction(
                name_from, 0, 0, rd, 0
            );

            test::check_error(
                test::utils::memory::write(mem, start_addr, 2, instructions)
            );

            // Prepare the environment
            test::check_error(mips_cpu_set_pc(cpu, start_addr));
            test::check_error(mips_cpu_set_register(cpu, rs, rs_value));

            // step through twice
            test::check_error(mips_cpu_step(cpu));

            // invalidate the current rs value
            test::check_error(mips_cpu_set_register(cpu, rs, ~rs_value));

            // step again
            test::check_error(mips_cpu_step(cpu));

            // check value if it correct, and set the relevant flags
            test::check_error(mips_cpu_get_register(cpu, rd, &actual_output));
            is_successful = (expected_output == actual_output);

        } catch(test::Exception e) {
            is_successful = false;
        }

        // execute the two tests
        test_id = mips_test_begin_test(name_from.c_str());
        mips_test_end_test(test_id, is_successful, NULL);
        test_id = mips_test_begin_test(name_to.c_str());
        mips_test_end_test(test_id, is_successful, NULL);
        mips_cpu_reset(cpu);
    }

    for (uint32_t i = 1 ; i < (1 << 5) ; i+= 8) {
        for (uint32_t j = 1 ; j < (1 << 5) ; j += 8) {
            for (uint32_t k = 1 ; k < (1 << 5); k += 8) {
                uint32_t instruction;
                uint32_t rs = 1, rd = 2;
                mips_error err;
                int test_id;

                // test the movefrom version 
                test_id = mips_test_begin_test(name_from.c_str());
                instruction = construct_instruction(
                    name_from, rs, i, j, k
                );
                test::utils::memory::write(mem, start_addr, 1, &instruction);
                mips_cpu_set_pc(cpu, start_addr);
                err = mips_cpu_step(cpu);
                mips_test_end_test(test_id,
                    err == mips_ExceptionInvalidInstruction, NULL);
                // end of movefrom test

                // test the moveto version
                test_id = mips_test_begin_test(name_to.c_str());
                instruction = construct_instruction(
                    name_to, i, j, rd, k
                );
                test::utils::memory::write(mem, start_addr, 1, &instruction);
                mips_cpu_set_pc(cpu, start_addr);
                err = mips_cpu_step(cpu);
                mips_test_end_test(test_id,
                    err == mips_ExceptionInvalidInstruction, NULL);
                // end of moveto test
            }
        }
    }

    mips_cpu_free(cpu);
    mips_mem_free(mem);
}

static void test_arithmetic(const string & name) {
    // Some arbitary constants
    const uint32_t start_addr = 0x120;

    // As we are unit testing, use a separate CPU for every test
    mips_mem_h mem = mips_mem_create_ram(0x20000, 4);
    mips_cpu_h cpu = mips_cpu_create(mem);
    mips_cpu_set_debug_level(cpu, 0, stdout);
    string filename(test::utils::make_file_name(name));
    std::ifstream fin(filename);
    TEST_CHECK_FILE_OPEN(fin, name);

    while (1) {

        // Reset the CPU before we start a next test case
        // Construct our own test case here
        // input plaeholders

        uint32_t rs, rt, rd,
                 rs_value, rt_value, shift,
                 expected_output, actual_output,
                 instruction;
        bool is_successful, is_expect_overflow;
        string s_tmp;
        mips_error err;


        if (name == "SLL" || name == "SRL" || name == "SRA") {
            rt = test::utils::fetch_integer(fin);
            rt_value = test::utils::fetch_integer(fin);
            shift = test::utils::fetch_integer(fin);
            rd = test::utils::fetch_integer(fin);
            expected_output = test::utils::fetch_integer(fin);
            is_expect_overflow = false;
            rs_value = 0;
            rs = 0;

        } else {
            rs = test::utils::fetch_integer(fin);
            rs_value = test::utils::fetch_integer(fin);
            rt = test::utils::fetch_integer(fin);
            rt_value = test::utils::fetch_integer(fin);
            rd = test::utils::fetch_integer(fin);
            fin >> s_tmp;

            test::utils::parse_arithmetic_input(
                s_tmp,
                &is_expect_overflow,
                &expected_output
            );
            shift = 0;
        }

        if (fin.eof()) {
            break;
        }

        int test_id = mips_test_begin_test(name.c_str());

        try {
            instruction = construct_instruction(name, rs, rt, rd, shift);

            // Write instruction to memory
            test::check_error(
                test::utils::memory::write(mem, start_addr, 1, &instruction)
            );

            // Prepare the environment
            test::check_error(mips_cpu_set_pc(cpu, start_addr));
            test::check_error(mips_cpu_set_register(cpu, rs, rs_value));
            test::check_error(mips_cpu_set_register(cpu, rt, rt_value));

            // Exectue the instruction
            err = mips_cpu_step(cpu);
            test::check_error(mips_cpu_get_register(cpu, rd, &actual_output));

            // determine whether it is successful
            is_successful = 
                (!err && expected_output == actual_output && !is_expect_overflow) ||
                (is_expect_overflow && err == mips_ExceptionArithmeticOverflow);

        } catch (test::Exception e) {
            is_successful = false;
        }

        // Reset CPU after iteration, prepare for next round of execution
        mips_cpu_reset(cpu);

        // Compare the results, if it is not good, we append 'i' to failed test cases
        mips_test_end_test(
            test_id,
            is_successful,
            NULL
        );
    }

    // Close the file stream
    fin.close();

    // Some weird bogus test cases
    // 1. Let shift be something that is not 0x1F 
    // expected results - mips_ExceptionInvalidInstruction

    for (uint32_t i = 1 ; i < (1 << 5) ; i++) {
        mips_error err;
        int test_id;
        uint32_t instruction;
        bool flag;

        if (name == "SLL" || name == "SRA" || name == "SRL") {
            instruction = construct_instruction(name, i, 3, 4, 2);
        } else {
            instruction = construct_instruction(name, 2, 3, 4, i);
        }

        try {
            test_id = mips_test_begin_test(name.c_str());
            test::check_error(
                test::utils::memory::write(mem, start_addr, 4, &instruction)
            );
            test::check_error(mips_cpu_set_pc(cpu, start_addr));
            err = mips_cpu_step(cpu);
            flag = (err == mips_ExceptionInvalidInstruction);
        } catch (test::Exception e) {
            flag = false;
        }

        mips_test_end_test(test_id, flag, NULL);
    }
}

static void test_multdiv(const string & name) {
    // Some arbitary constants
    const uint32_t start_addr = 0x120;

    // As we are unit testing, use a separate CPU for every test
    mips_mem_h mem = mips_mem_create_ram(0x20000, 4);
    mips_cpu_h cpu = mips_cpu_create(mem);
    mips_cpu_set_debug_level(cpu, 0, stdout);
    std::ifstream fin(test::utils::make_file_name(name));
    TEST_CHECK_FILE_OPEN(fin, name);

    // read the number of test cases
    while (1) {

        // Reset the CPU before we start a next test case
        // Construct our own test case here

        // input plaeholders
        uint32_t rs, rt, r_lo, r_hi,
                 rs_value, rt_value,
                 expected_lo, expected_hi,
                 actual_lo, actual_hi;
        std::string tmp;
        uint32_t instruction[5];
        int test_id;
        bool is_ok_anything = false;
        bool is_successful = true;

        rs = test::utils::fetch_integer(fin);
        rs_value = test::utils::fetch_integer(fin);
        rt = test::utils::fetch_integer(fin);
        rt_value = test::utils::fetch_integer(fin);

        // lo
        r_lo = test::utils::fetch_integer(fin);
        fin >> tmp;
        test::utils::parse_multdiv_input(tmp, &is_ok_anything, &expected_lo);

        // hi
        r_hi = test::utils::fetch_integer(fin);
        fin >> tmp;
        test::utils::parse_multdiv_input(tmp, &is_ok_anything, &expected_hi);

        if (fin.eof()) {
            break;
        }

        try {
            test_id = mips_test_begin_test(name.c_str());

            instruction[0] = construct_instruction(name, rs, rt, 0ul, 0ul);
            instruction[1] = 0;
            instruction[2] = 0;
            instruction[3] = construct_instruction("MFHI", 0, 0, r_hi, 0ul);
            instruction[4] = construct_instruction("MFLO", 0, 0, r_lo, 0ul);

            // write instruction into memory
            test::check_error(
                test::utils::memory::write(mem, start_addr, 5, instruction)
            );

            // Prepare the environment
            test::check_error(mips_cpu_set_pc(cpu, start_addr));
            test::check_error(mips_cpu_set_register(cpu, rs, rs_value));
            test::check_error(mips_cpu_set_register(cpu, rt, rt_value));

            // Exectue the instruction
            for (int i = 0 ; i < 5 ; i++) {
                test::check_error(mips_cpu_step(cpu));
            }

            test::check_error(mips_cpu_get_register(cpu, r_lo, &actual_lo));
            test::check_error(mips_cpu_get_register(cpu, r_hi, &actual_hi));

            // determine whether it is successful
            // (don't do anything if it has already failed prior to this)
            //
            is_successful = (is_ok_anything) || (((expected_lo == actual_lo) &&
                            (expected_hi == actual_hi)));

            // Compare the results, if it is not good, we append 'i' to failed test cases
        } catch (test::Exception e) {
            is_successful = false;
        }

        mips_test_end_test(
            test_id,
            is_successful,
            NULL
        );

        // Reset CPU after every iteration, prepare for next round of execution
        mips_cpu_reset(cpu);
    }

    // Close the file stream
    fin.close();

    // Some weird bogus test cases
    // 1. Let shift be something that is not 0x1F 
    // expected results - mips_ExceptionInvalidInstruction

    for (uint32_t i = 1 ; i < (1 << 5) ; i++) {
        uint32_t instruction = construct_instruction(name, 2, 3, 4, i);
        mips_error err;
        int test_id;

        test::utils::memory::write(mem, start_addr, 1, &instruction);

        test_id = mips_test_begin_test(name.c_str());
        mips_cpu_set_pc(cpu, start_addr);
        err = mips_cpu_step(cpu);
        mips_test_end_test(
            test_id,
            (err == mips_ExceptionInvalidInstruction),
            NULL
        );
    }

    mips_cpu_free(cpu);
    mips_mem_free(mem);
}

void test_jump (const string & name) {
    mips_cpu_h cpu;
    mips_mem_h mem;
    std::ifstream fin(test::utils::make_file_name(name));
    TEST_CHECK_FILE_OPEN(fin, name);
    mem = mips_mem_create_ram(0x20000, 4);
    cpu = mips_cpu_create(mem);

    while(1) {
        int test_id;
        uint32_t start_addr, pc;
        uint32_t instruction[3];
        uint32_t rs, rs_value, rt, shift, rd;
        bool is_successful = true;
        bool is_link;
        start_addr = 0x120;

        shift = 0;
        rt = 0;

        rs = test::utils::fetch_integer(fin);
        rs_value = test::utils::fetch_integer(fin);

        if (name == "JR") {
            rd = 0;
            is_link = false;

        } else if (name == "JALR") {
            rd = test::utils::fetch_integer(fin);
            is_link = true;

        } else {
            cerr << "Unknown Jump RInstruction" << endl;
            exit(1);
        }

        if (fin.eof()) {
            break;
        }

        // TODO : More weird test cases

        try {
            // init the CPU and memory
            test_id = mips_test_begin_test(name.c_str());
            // Prepare the configuration environment

            instruction[0] = construct_instruction(name, rs, rt, rd, shift);
            instruction[1] = 0b100000;
            test::check_error(
                test::utils::memory::write(mem, start_addr, 2, instruction)
            );
            test::check_error(mips_cpu_set_debug_level(cpu, 0, stdout));
            test::check_error(mips_cpu_set_pc(cpu, start_addr));
            test::check_error(mips_cpu_set_register(cpu, rs, rs_value));

            test::check_error(mips_cpu_step(cpu));
            test::check_error(mips_cpu_get_pc(cpu, &pc));
            test::assert(pc == start_addr + 4);

            if (is_link) {
                uint32_t tmp = 0;
                test::check_error(mips_cpu_get_register(cpu, rd, &tmp));
                test::assert(
                    (rd == 0 && tmp == 0) || (tmp == start_addr + 8)
                );
            }

            test::check_error(mips_cpu_step(cpu));
            test::check_error(mips_cpu_get_pc(cpu, &pc));

            // set the is_successful flag
            if (rs == 0) {
                is_successful = (pc == 0);
            } else  {
                is_successful = (pc == rs_value);
            }

        } catch (test::Exception e) {
            is_successful = false;
        }

        mips_test_end_test(
            test_id,
            is_successful,
            NULL
        );
    }

    fin.close();
}

