#include "test.h"
using namespace std;
static map<string, uint32_t> m_constants;

// ==========================
// Function Prototypes or global buffers
// ==========================

static void init();
static uint32_t construct_instruction(const string &, const uint32_t);
static void test_single_jump(const string &);

// ==========================
// Main (main) Body
// ==========================

void test_jinstructions() {
    init(); 
    test_single_jump("J");
    test_single_jump("JAL");
}

// ==========================
// Function Declaration 
// ==========================

void init() {
    static bool init_ = false;
    if (init_) return;
    // constant declrations here

    m_constants["JAL"] = 0b000011;
    m_constants["J"] = 0b000010;

    // end of declarations

    init_ = true;
}

static uint32_t construct_instruction(
    const string & name,
    const uint32_t target 
) {
    return (
        ((m_constants[name] & 0x3F) << 26) |
        (target & 0x3FFFFFF)
    );
}

void test_single_jump(const string & name) {
    mips_cpu_h cpu;
    mips_mem_h mem;
    const uint32_t start_addr = 0x120;
    std::ifstream fin(test::utils::make_file_name(name));
    TEST_CHECK_FILE_OPEN(fin, name);

    mem = mips_mem_create_ram(0x20000, 4);
    cpu = mips_cpu_create(mem);
    mips_cpu_set_debug_level(cpu, 0, stdout);

    while(1) {
        uint32_t target, pc;
        uint32_t instruction[3];
        bool is_successful = true;
        bool is_link;
        int test_id;

        fin >> target;
        if (fin.eof()) {
            break;
        }

        try {
            test_id = mips_test_begin_test(name.c_str());
            is_link = (name == "JAL");

            // Prepare the configuration environment
            instruction[0] = construct_instruction(
                name, target 
            );
            instruction[1] = 0b100000;
            test::check_error(
                test::utils::memory::write(mem, start_addr, 2, instruction)
            );
            test::check_error(mips_cpu_set_pc(cpu, start_addr));

            test::check_error(mips_cpu_step(cpu));
            test::check_error(mips_cpu_get_pc(cpu, &pc));
            test::assert(pc == start_addr + 4);

            if (is_link) {
                uint32_t tmp = 0;
                test::check_error(mips_cpu_get_register(cpu, 31, &tmp));
                test::assert(tmp == start_addr + 8);
            }

            test::check_error(mips_cpu_step(cpu));
            test::check_error(mips_cpu_get_pc(cpu, &pc));
            is_successful = (pc == (target << 2));

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

