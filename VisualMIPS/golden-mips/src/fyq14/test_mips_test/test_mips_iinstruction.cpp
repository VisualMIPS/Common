#include "test.h"
using namespace std;

// ==========================
// Fucntion Prototypes
// ==========================

static map<string, uint32_t> m_constants;
static void init();
static uint32_t construct_branch_instruction(
    string name,
    uint32_t rs,
    uint32_t rt,
    const uint16_t offset
);
static uint32_t construct_memory_instruction(
    string, uint32_t, uint32_t, uint32_t
);
static void test_memory_store(string name);
static void test_memory_load(string name);
static void test_single_branch(string name);
static void test_immediate_arithmetic(
    string name
);
static void test_load_upper_intermediate();


// ==========================
// Main (main) Body
// ==========================

void test_iinstructions() {
    init();

    // Memory and friends
    test_memory_load("LW");
    test_memory_load("LB");
    test_memory_load("LBU");
    test_memory_load("LH");
    test_memory_load("LHU");
    test_memory_load("LWL");
    test_memory_load("LWR");
    test_memory_store("SW");
    test_memory_store("SB");
    test_memory_store("SH");
    test_load_upper_intermediate();

    // Branches
    test_single_branch("BNE");
    test_single_branch("BEQ");
    test_single_branch("BGEZAL");
    test_single_branch("BGEZ");
    test_single_branch("BLTZ");
    test_single_branch("BLTZAL");
    test_single_branch("BGTZ");
    test_single_branch("BLEZ");

    // Arithmetic
    test_immediate_arithmetic("ANDI");
    test_immediate_arithmetic("XORI");
    test_immediate_arithmetic("ORI");
    test_immediate_arithmetic("ADDIU");
    test_immediate_arithmetic("ADDI");
    test_immediate_arithmetic("SLTI");
    test_immediate_arithmetic("SLTIU");
}

// =============================
// initializations
// =============================

static void init() {
    static bool init_ = false;
    if (init_) return;

    // Memory and friends
    m_constants["LB"] = 0b100000;
    m_constants["LBU"] = 0b100100;
    m_constants["LH"] = 0b100001;
    m_constants["LHU"] = 0b100101;
    m_constants["LW"] = 0b100011;
    m_constants["SB"] = 0b101000;
    m_constants["SH"] = 0b101001;
    m_constants["SW"] = 0b101011;

    // LUI
    m_constants["LUI"] = 0b001111;

    // Level 5 stuff
    m_constants["LWR"] = 0b100110;
    m_constants["LWL"] = 0b100010;

    // Immediate Arithmethic
    m_constants["ADDI"] = 0b001000;
    m_constants["ADDIU"] = 0b001001;
    m_constants["ANDI"] = 0b001100;
    m_constants["ORI"] = 0b001101;
    m_constants["XORI"] = 0b001110;
    m_constants["SLTI"] = 0b001010;
    m_constants["SLTIU"] = 0b001011;

    // Branches
    m_constants["BEQ"] = 0b000100;
    m_constants["BNE"] = 0b000101;
    m_constants["REGIMM"] = 0b000001;
    m_constants["BGEZ"] = 0b00001;
    m_constants["BLTZ"] = 0b00000;
    m_constants["BLTZAL"] = 0b10000;
    m_constants["BGEZAL"] = 0b10001;
    m_constants["BGTZ"] = 0b000111;
    m_constants["BLEZ"] = 0b000110;

    init_ = true;
}
// ==========================
// Function Definitions
// ==========================

uint32_t construct_memory_instruction(
    string name,
    uint32_t base,
    uint32_t rt,
    uint32_t offset
) {
    return (
        ((m_constants[name] & 0x3F) << 26) |
        ((base & 0x1F) << 21) |
        ((rt & 0x1F) << 16) |
        (offset & 0xFFFF)
    );
}

static void test_load_upper_intermediate() {
    const string name = "LUI";
    const uint32_t addr = 0x120;
    mips_mem_h mem = mips_mem_create_ram(0x20000, 4);
    mips_cpu_h cpu = mips_cpu_create(mem);
    ifstream fin(test::utils::make_file_name(name)); 

    while(1) {
        uint32_t rt,
                 konst,
                 instruction,
                 actual_results;
        bool is_successful = false;

        rt = test::utils::fetch_integer(fin);
        konst = test::utils::fetch_integer(fin);

        if (fin.eof()) break;

        instruction = construct_memory_instruction(
            name,
            0,
            rt,
            konst
        );

        int test_id = mips_test_begin_test(name.c_str());

        try {
            test::check_error(
                test::utils::memory::write(mem, addr, 1, &instruction)
            );
            test::check_error(mips_cpu_set_pc(cpu, addr));
            test::check_error(mips_cpu_step(cpu));
            test::check_error(mips_cpu_get_register(cpu, rt, &actual_results));

            is_successful = ((rt == 0 && actual_results == 0) ||
                    (actual_results == (konst << 16)));

        } catch (test::Exception e) {
            is_successful = false;
        }

        mips_test_end_test(test_id, is_successful, NULL);
    }

    // check for bogus instructions
    for (uint32_t rs = 1 ; rs <= 31 ; rs++) {
        int test_id = mips_test_begin_test(name.c_str());
        uint32_t instruction = construct_memory_instruction(
            name, rs, 2, 0xFF 
        );
        test::utils::memory::write(mem, addr, 1, &instruction);
        mips_cpu_set_pc(cpu, addr);
        mips_error err = mips_cpu_step(cpu);
        mips_test_end_test(test_id,
            err == mips_ExceptionInvalidInstruction, NULL);
    }

    mips_cpu_free(cpu);
    mips_mem_free(mem);
}

static void test_memory_store(string name) {
    mips_mem_h mem = mips_mem_create_ram(0x20000, 4);
    mips_cpu_h cpu = mips_cpu_create(mem);
    const uint32_t start_addr = 0x120;
    std::ifstream fin(test::utils::make_file_name(name));

    while (1) {
        uint32_t rs, rs_value,
                 rt, rt_value,
                 offset, instruction;
        uint8_t buffer[8];
        mips_error err;
        bool is_successful = true;
        bool is_expect_allignment;
        bool is_expect_okay;
        int test_id;
        string s_tmp;

        // initialize the variables here
        rs = test::utils::fetch_integer(fin);
        rs_value = test::utils::fetch_integer(fin);
        rt = test::utils::fetch_integer(fin);
        rt_value = test::utils::fetch_integer(fin);
        offset = test::utils::fetch_integer(fin);
        fin >> s_tmp;
        is_expect_okay = (s_tmp == "OKAY");
        is_expect_allignment= (s_tmp == "ALLIGNMENT");

        if (fin.eof()) { break; }

        try {
            test_id = mips_test_begin_test(name.c_str());
            instruction = construct_memory_instruction(
                name,
                rs,
                rt,
                offset
            );

            // Initialize the relevant registers and/or PC
            test::check_error(
                test::utils::memory::write(mem, start_addr, 1, &instruction)
            );
            mips_cpu_set_debug_level(cpu, 0, stdout);
            mips_cpu_set_pc(cpu, start_addr);
            mips_cpu_set_register(cpu, rs, rs_value);
            mips_cpu_set_register(cpu, rt, rt_value);
            uint32_t zeros[2] = { 0, 0 };
            test::check_error(
                test::utils::memory::write(
                    mem, rs_value - (rs_value % 4), 2, zeros
                )
            );

            // run test!
            err = mips_cpu_step(cpu);

            uint32_t data_addr;
            if (rs == 0) {
                data_addr = offset;
            } else {
                data_addr = offset + rs_value;
            }

            uint32_t read_offset  = data_addr % 4;
            test::check_error(mips_mem_read(mem, data_addr - read_offset, 8, buffer));

            if (is_expect_allignment) {
                is_successful = (err == mips_ExceptionInvalidAlignment);
            } else if (is_expect_okay) {
                uint32_t expected_value;
                if (rt == 0) {
                    expected_value = 0;
                } else {
                    expected_value = rt_value;
                }

                if (name == "SW") {
                    is_successful = (!err &&
                            buffer[read_offset]   == (0xFF & (expected_value >> 24)) &&
                            buffer[read_offset+1] == (0xFF & (expected_value >> 16)) &&
                            buffer[read_offset+2] == (0xFF & (expected_value >> 8)) &&
                            buffer[read_offset+3] == (0xFF & (expected_value >> 0)));
                } else if (name == "SH") {
                    is_successful = (!err &&
                            buffer[read_offset]   == (0xFF & (expected_value >> 8)) &&
                            buffer[read_offset+1] == (0xFF & (expected_value >> 0)));
                } else if (name == "SB") {
                    is_successful = (!err && buffer[read_offset] == (0xFF & expected_value));
                }
            } else {
                cerr << "LOL?" << endl;
                exit(1);
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

    mips_cpu_free(cpu);
    mips_mem_free(mem);
}

static void test_memory_load(string name) {

    ifstream fin(test::utils::make_file_name(name));
    TEST_CHECK_FILE_OPEN(fin, name);

    mips_mem_h mem = mips_mem_create_ram(0x20000, 4);
    mips_cpu_h cpu = mips_cpu_create(mem);

    while(1) {
        uint32_t rs, rs_value,
                 rt, rt_value,
                 instruction,
                 start_addr = 0x120,
                 actual_output, expected_output;
        uint8_t * buffer;
        uint32_t offset;
        bool is_successful = true, is_expected_allignment = false;
        int N;
        uint32_t data_addr;
        string s_tmp;
        mips_error err;
    
        fin >> data_addr >> N;

        if (fin.eof()) break;

        if (N % 4 != 0) {
            cerr << "Bogus test case!" << endl;
            exit(1);
        }

        buffer = static_cast<uint8_t*>(calloc(N, sizeof(uint8_t)));

        for (int i = 0 ; i < N ; i++) {
            buffer[i] = uint8_t(test::utils::fetch_integer(fin));
        }

        // initialize the variables here
        rs              = test::utils::fetch_integer(fin);
        rs_value        = test::utils::fetch_integer(fin);
        rt              = test::utils::fetch_integer(fin);

        if (name == "LWL" || name == "LWR") {
            rt_value = test::utils::fetch_integer(fin);
        } else {
            // set rt_value to arbitary value, just to make sure
            // compilers don't yield strange warnings
            // (I KNOW WHAT I AM DOING MATE)
            rt_value = 0;
        }
        offset          = test::utils::fetch_integer(fin);
        fin >> s_tmp;
        if (s_tmp == "ALLIGNMENT") {
            is_expected_allignment = true;
        } else {
            is_expected_allignment = false;
            expected_output = test::utils::from_string<uint32_t>(s_tmp);
        }

        instruction = construct_memory_instruction(
            name,
            rs,
            rt,
            offset
        );

        // begin test!
        int test_id = mips_test_begin_test(name.c_str());

        try {
            // Initialize memory : populate the memory data as appropriate
            if(N != 0) {
                test::check_error(
                    mips_mem_write(mem, data_addr, N, buffer)
                );
            }

            // Initialize cpu : populate the relevant registers and/or PC
            test::check_error(
                test::utils::memory::write(mem, start_addr, 1, &instruction)
            );
            test::check_error(mips_cpu_set_debug_level(cpu, 0, stdout));
            test::check_error(mips_cpu_set_pc(cpu, start_addr));
            test::check_error(mips_cpu_set_register(cpu, rs, rs_value));
            test::check_error(mips_cpu_set_register(cpu, rt, rt_value));
            err = mips_cpu_step(cpu);
            test::check_error(mips_cpu_get_register(cpu, rt, &actual_output));

            is_successful = ((is_expected_allignment && err == mips_ExceptionInvalidAlignment) || 
                                (actual_output == expected_output));

        } catch (test::Exception e) {
            is_successful = false;

        }

        mips_test_end_test(
            test_id,
            is_successful,
            NULL
        );

        free(buffer);
        mips_cpu_reset(cpu);
    }

    mips_cpu_free(cpu);
    mips_mem_free(mem);
}

static uint32_t construct_branch_instruction(
    string name,
    uint32_t rs,
    uint32_t rt,
    const uint16_t offset
) {
    if (name == "BEQ" || name == "BNE" ||
            name == "BGTZ" || name == "BLEZ") {
        return (
            ((m_constants[name] & 0x3F) << 26) |
            ((rs & 0x1F) << 21) |
            ((rt & 0x1F) << 16) |
            (offset & 0xFFFF)
        );
    } else if (name == "BGEZ" || "BLTZ" == name ||
                name == "BGEZAL" || "BLTZAL" == name) {
        return (
            ((m_constants["REGIMM"] & 0x2F) << 26) |
            ((rs & 0x1F) << 21) |
            ((m_constants[name] & 0x1F) << 16) |
            ((offset & 0xFFFF) << 0)
        );
    }

    cerr << "Unkown branch instruction" << endl;
    exit(1);
}



static void test_single_branch(string name) {
    std::ifstream fin(test::utils::make_file_name(name));
    TEST_CHECK_FILE_OPEN(fin, name);
    mips_mem_h mem = mips_mem_create_ram(0x20000, 4);
    mips_cpu_h cpu = mips_cpu_create(mem);
    mips_cpu_set_debug_level(cpu, 0, stdout);
    uint32_t start_addr = 0x120;

    while (!fin.eof()) {
        uint32_t rs, rt, offset,
                 rs_value, rt_value;
        uint32_t instruction_start[3];
        uint32_t pc;
        int test_id;
        bool is_successful = true,
             should_branch,
             is_branch_link = false;

        // Read values from files / initializations
        is_branch_link = (name == "BGEZAL") || (name == "BLTZAL");

        if (name == "BNE" || name == "BEQ") {
            rs = test::utils::fetch_integer(fin);
            rs_value = test::utils::fetch_integer(fin);
            rt = test::utils::fetch_integer(fin);
            rt_value = test::utils::fetch_integer(fin);
            offset = test::utils::fetch_integer(fin);
            should_branch = static_cast<bool>(test::utils::fetch_integer(fin));

        } else {
            rt = 0;
            rt_value = 0;
            rs = test::utils::fetch_integer(fin);
            rs_value = test::utils::fetch_integer(fin);
            offset = test::utils::fetch_integer(fin);
            should_branch = static_cast<bool>(test::utils::fetch_integer(fin));
        }

        // EOF:
        if (fin.eof()) { break; }

        // start the test and get the test_id
        test_id = mips_test_begin_test(name.c_str());

        try {
            
            instruction_start[0] = construct_branch_instruction(
                name, rs, rt, offset
            );
            instruction_start[1] = 0b100000;

            // init environment
            test::check_error(
                test::utils::memory::write(mem, start_addr, 2, instruction_start)
            );
            test::check_error(mips_cpu_set_register(cpu, rs, rs_value));
            test::check_error(mips_cpu_set_register(cpu, rt, rt_value));
            test::check_error(mips_cpu_set_pc(cpu, start_addr));

            // STEP 1 
            test::check_error(mips_cpu_step(cpu));
            test::check_error(mips_cpu_get_pc(cpu, &pc));
            test::assert(pc == start_addr + 4);

            if (is_branch_link) {
                uint32_t tmp = 0;
                test::check_error(mips_cpu_get_register(cpu, 31, &tmp));
                test::assert(tmp == start_addr + 8);
            }

            // STEP 2
            test::check_error(mips_cpu_step(cpu));
            test::check_error(mips_cpu_get_pc(cpu, &pc));

            if (should_branch) {
                is_successful = (pc == start_addr + 4 + (test::utils::sign_extend_16(uint16_t(offset & 0xFFFFFF)) << 2));
            } else {
                is_successful = (pc == start_addr + 8);
            }

            if (!is_successful) {
                cout << "rs = " << rs << ", rt = " << rt << endl;
                cout << "rs_value = " << rs_value << ", rt_value = " << rt_value << endl;
                cout << "pc = " << pc << endl;
                cout << "should_branch = " << should_branch << endl;
                cout << "start_addr + 4 + (offset << 2) = " << start_addr + 4 + (test::utils::sign_extend_16(uint16_t(offset & 0xFFFFFF)) << 2) << endl;
                cout << "start_addr + 8 = " << start_addr << endl;
                cout << endl;
            }

        } catch(test::Exception e) {
            is_successful = false;
        }

        // end the test and report result
        mips_test_end_test(
            test_id,
            is_successful,
            NULL
        );
    }

    fin.close();

    // if it is BGTZ or BLEZ, permutate rt to make sure a non-zero value
    // if deemed as invalid instruction
    if (name == "BGTZ" || name == "BLEZ") {
        for (uint32_t i = 1 ; i < 32 ; i++) {
            uint32_t rs = 1;
            uint32_t rt = i;
            uint32_t offset = 123;
            uint32_t instruction = construct_branch_instruction(
                name, rs, rt, offset
            );
            int test_id = mips_test_begin_test(name.c_str());

            try {
                mips_error err;

                test::check_error(
                    test::utils::memory::write(mem, start_addr, 1, &instruction)
                );
                test::check_error(
                    mips_cpu_set_pc(cpu, start_addr)
                );
                err = mips_cpu_step(cpu);
                mips_test_end_test(
                    test_id,
                    err == mips_ExceptionInvalidInstruction,
                    ""
                );
            } catch (test::Exception e) {
                mips_test_end_test(test_id, false, "");
            }
        }
    }

    mips_cpu_free(cpu);
    mips_mem_free(mem);
}

static void test_immediate_arithmetic(string name) {
    const uint32_t start_addr = 0x120;
    std::ifstream fin(test::utils::make_file_name(name));
    TEST_CHECK_FILE_OPEN(fin, name);

    mips_mem_h mem = mips_mem_create_ram(0x20000, 4);
    mips_cpu_h cpu = mips_cpu_create(mem);
    mips_cpu_set_debug_level(cpu, 0, stdout);

    while (true) {
        int test_id;
        uint32_t rs, rt,
                 rs_value, immediate,
                 expected_output, actual_output,
                 instruction;
        bool is_successful, is_expect_overflow;
        string s_tmp;
        mips_error err;

        rs = test::utils::fetch_integer(fin);
        rs_value = test::utils::fetch_integer(fin);
        rt = test::utils::fetch_integer(fin);
        immediate = test::utils::fetch_integer(fin);

        fin >> s_tmp;
        test::utils::parse_arithmetic_input(s_tmp,
            &is_expect_overflow, &expected_output);

        if (fin.eof()) { break; }

        try {
            test_id = mips_test_begin_test(name.c_str());

            // Init environment
            instruction =
                (((m_constants[name] & 0x3F) << 26) |
                ((rs & 0x1F) << 21) |
                ((rt & 0x1F) << 16) |
                (immediate & 0xFFFF));

            // Write instruction to memory
            test::check_error(
                test::utils::memory::write(mem, start_addr, 1, &instruction)
            );

            // Prepare the environment
            test::check_error(mips_cpu_set_pc(cpu, start_addr));
            test::check_error(mips_cpu_set_register(cpu, rs, rs_value));

            // execute the instruction
            err = mips_cpu_step(cpu);
            test::check_error(mips_cpu_get_register(cpu, rt, &actual_output));

            // determine whether it is successful
            is_successful =
                (!err && expected_output == actual_output &&
                    !is_expect_overflow) ||
                (is_expect_overflow &&
                    err == mips_ExceptionArithmeticOverflow);

            // Reset CPU after iteration, prepare for next round of execution
            mips_cpu_reset(cpu);

            if (!is_successful) {
                cout << endl;
                cout << "=================" << endl;
                cout << "rs(" << rs << ") = " << uint32_t(rs_value) << endl;
                cout << "konst = " << uint32_t(immediate) << endl;
                cout << "expect overflow? = " << is_expect_overflow << endl;
                cout << "err = " << err << endl;
                cout << "rt = " << rt << endl;
                cout << "Expected output = " << expected_output << endl;
                cout << "Actual output = " << actual_output << endl;
                cout << "=================" << endl;
                cout << endl;
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

    mips_cpu_free(cpu);
    mips_mem_free(mem);
}
