// the purpose of this file is to run unit test on the classes defined 
// by my implementation
// This is by no means actual source code (for the graded test suite or
// implementation). Hence, it is not meant to be run by the grader.
// Same goes to the family of file whose name starts with the prefox unit_test*.cpp

#include "unit_test/test_suite.h"
#include "unit_test/test_register.h"
#include "unit_test/test_memory.h"
#include "unit_test/test_fetcher.h"
#include "unit_test/test_instruction.h"
#include "main/MIPS/instruction_decoder.h"
#include "main/MIPS/cpu.h"

int main() {
    unit_test_register();
    unit_test_memory();
    unit_test_fetcher();
    unit_test_instruction();

    mips_mem_h mem = mips_mem_create_ram(8000, 4);
    mips_cpu_impl cpu(mem);
    MIPS::InstructionDecoder decoder(cpu);
    instruction_t bits = 0x12311231;
    MIPS::Instruction * instruction_ptr;
    decoder << bits;
    decoder >> instruction_ptr;
    instruction_ptr->execute();
    
}
