#include "MIPS/register.h"
#include "MIPS/cpu.h"

namespace MIPS {
    // ------------------------------
    // Register Interface definitions
    // ------------------------------

    RegisterInterface::RegisterInterface() : ptr_cpu(NULL) {}
    RegisterInterface::~RegisterInterface() {}

    void RegisterInterface::set_ptr_cpu(mips_cpu_impl * ptr_cpu_in) {
        ptr_cpu = ptr_cpu_in;
    }

    // ------------------------------
    // Regular Register definitions
    // ------------------------------

    RegularRegister::RegularRegister() {
        id = -1;
        value = 0; 
    }

    RegularRegister::RegularRegister(uint32_t a) {
        id = -1;
        value = a;
    }

    void RegularRegister::set_index(int id_in) {
        id = id_in;
    }

    uint32_t RegularRegister::get() const {
        return value;
    }

    void RegularRegister::set(uint32_t set_value) {
        if (ptr_cpu != NULL) {
            switch (id) {
            case ID_LO:
                ptr_cpu->logger.debug("    => Setting LO Register from 0x%.8X to 0x%.8x\n",
                    value, set_value);
                break;

            case ID_HI:
                ptr_cpu->logger.debug("    => Setting HI Register from 0x%.8X to 0x%.8x\n",
                    value, set_value);
                break;

            case ID_PC:
                ptr_cpu->logger.debug("    => Setting Program Counter (PC) from 0x%.8X to 0x%.8x\n",
                    value, set_value);
                break;

            default:
                ptr_cpu->logger.debug("    => Setting Register %d from 0x%.8X to 0x%.8x\n",
                    id, value, set_value);
                break;
            }
        }
        value = set_value;
    }

    // ------------------------------
    // Zero Register definitions
    // ------------------------------

    ZeroRegister::ZeroRegister(){}
    ZeroRegister::ZeroRegister(uint32_t a){
        (void) a;
    }

    uint32_t ZeroRegister::get() const {
        return 0;
    }

    void ZeroRegister::set(uint32_t set_value) {
        if (ptr_cpu != NULL) {
            ptr_cpu->logger.debug("    => Setting Zero Register from 0 to 0\n");
        }
        // Do nothing 
        (void) set_value;
    }
}
