#include "test_instruction.h"
#include "../main/MIPS/functions.h"
#include "../main/MIPS/instruction.h"
using namespace std;

void unit_test_instruction() {
    MIPS::RegisterInterface *rs = new MIPS::RegularRegister(),
        *rd = new MIPS::RegularRegister(),
        *rt = new MIPS::RegularRegister();
    MIPS::ArithmeticRInstruction * r_instruction =
        new MIPS::ArithmeticRInstruction(MIPS::functions::addition, false);
    r_instruction->set_rt(rt);
    r_instruction->set_rs(rs);
    r_instruction->set_rd(rd);
    MIPS::Instruction * instruction = static_cast<MIPS::Instruction*>(r_instruction);
    instruction->execute();
}

