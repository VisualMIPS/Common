#include "test_fetcher.h"
#include <iostream>
using namespace std;

static const addr_t start_address = 0x204;
static const instruction_t dummy_instructions[] = {
    0xFFFFFFFF,
    0x00556723,
    0x11223344,
    0x39478694,
    0x11112934,
    0x12200000
};

DEFTEST(instruction_fetcher_jump) {
    // need to make sure it jumps to the next memory address (check head using get_head())
    // and the buffer is completely flushed (i.e: not memory stall instructions)
    mips_mem_h mem_h = mips_mem_create_ram(8000, 4);
    MIPS::InstructionFetcher fetcher(mem_h);
    fetcher.jump(start_address);
    const addr_t jump_address = 100;
    for (addr_t i = 0 ; i < 3 ; i++) {
        MIPS::memory::write(
            mem_h, start_address + i * 4, 4,
            (uint8_t*) (dummy_instructions + i)
        );
    }
    for (addr_t i = 0 ; i < 3 ; i++) {
        MIPS::memory::write(
            mem_h, jump_address + i * 4, 4,
            (uint8_t*) (dummy_instructions + 3 + i)
        );
    }

    // Normal fetch
    for (int i = 0 ; i < 3 ; i++) {
        fetcher.fetch();
    }
    ASSERT(fetcher.buffer_size() == 3);
    ASSERT(fetcher.get_head() == start_address + 3 * 4);

    // Jump, make sure buffer is cleared and head is new
    fetcher.jump(jump_address);
    ASSERT(fetcher.buffer_size() == 0);
    ASSERT(fetcher.get_head() == jump_address);

    // Make sure we are actually fetching instruction from the jumped address
    for (int i = 3 ; i < 6 ; i++) {
        instruction_t bits;
        fetcher >> bits;
        ASSERT(bits == dummy_instructions[i]);
    }
}

DEFTEST(instruction_fetcher_right_shift_operator) {
    mips_mem_h mem_h = mips_mem_create_ram(8000, 4);
    MIPS::InstructionFetcher fetcher(mem_h);
    fetcher.jump(start_address);
    for (addr_t i = 0 ; i < 3 ; i++) {
        MIPS::memory::write(mem_h, start_address + i * 4, 4, (uint8_t*) (dummy_instructions + i));
    }

    for (int i = 0 ; i < 3 ; i++) {
        instruction_t bits;
        fetcher >> bits;
        ASSERT(bits == dummy_instructions[i]);
    }
}

DEFTEST(instruction_fetcher_fetch) {
    mips_mem_h mem_h = mips_mem_create_ram(8000, 4);
    MIPS::InstructionFetcher fetcher(mem_h);
    fetcher.jump(start_address);

    for (addr_t i = 0 ; i < 6 ; i++) {
        MIPS::memory::write(mem_h, start_address + i * 4, 4, (uint8_t*) (dummy_instructions + i));
    }

    for (uint32_t i = 0 ; i < 5 ; i++) {
        fetcher.fetch();
        ASSERT(fetcher.buffer_size() == i + 1);
    }
}

DEFTEST(instruction_fetcher_constructor) {
    mips_mem_h mem_h = mips_mem_create_ram(8000, 4);
    MIPS::InstructionFetcher fetcher(mem_h);
    fetcher.jump(0x940);
    ASSERT(fetcher.get_head() == 0x940);
}

void unit_test_fetcher() {
    RUNTEST(instruction_fetcher_constructor);
    RUNTEST(instruction_fetcher_jump);
    RUNTEST(instruction_fetcher_right_shift_operator);
    RUNTEST(instruction_fetcher_fetch);
}

