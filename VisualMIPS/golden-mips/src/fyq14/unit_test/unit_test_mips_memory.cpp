#include "test_memory.h"
#include "../main/MIPS/memory.h"

#include <iostream>

DEFTEST(memory) {
    // test write to memory
    mips_mem_h mem_h = mips_mem_create_ram(8000, 4);

    uint8_t data[4] = { 0xFF, 0x01, 0x02, 0x03 };
    uint8_t data_in[4];
    try {
        MIPS::memory::write(mem_h, 0, 4, data);
    } catch (MIPS::Exception & e) {
        // Oops, shouldn't have an exception here
        ASSERT(false);
    }

    // invalid address, expect InvalidAllignmentException 
    try {
        MIPS::memory::read(mem_h, 0, 5, data);
        ASSERT(false);
    } catch(MIPS::InvalidAlignmentException & e) {
        ASSERT(true);
    }

    // test read from memory
    MIPS::memory::read(mem_h, 0, 4, data_in);
    for (int i = 0 ; i < 4 ; i++) {
        ASSERT(data_in[i] == data[i]);
    }

    // free memory
    mips_mem_free(mem_h);
}

void unit_test_memory() {
    RUNTEST(memory);
}
