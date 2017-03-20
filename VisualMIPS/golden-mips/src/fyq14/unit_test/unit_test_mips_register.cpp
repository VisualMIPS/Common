#include "test_register.h"
#include "../main/MIPS/register.h"

DEFTEST(normal_register) {
    MIPS::RegisterInterface * r = new MIPS::RegularRegister(123);
    ASSERT(r->get() == 123);
    r->set(345);
    ASSERT(r->get() == 345);
    delete r;
}

DEFTEST(zero_register) {
    MIPS::RegisterInterface * r = new MIPS::ZeroRegister();
    ASSERT(r->get() == 0);
    r->set(345);
    ASSERT(r->get() == 0);
    delete r;
}

void unit_test_register() {
    RUNTEST(normal_register);
    RUNTEST(zero_register);
}

