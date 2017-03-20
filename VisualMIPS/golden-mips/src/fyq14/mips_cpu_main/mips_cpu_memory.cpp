#include "MIPS/memory.h"

namespace MIPS {
    namespace memory {
        void read(
            mips_mem_h handler,
            addr_t addr,
            addr_t length,
            mem_data_t * data_out
        ) {
            uint8_t * buffer = new uint8_t[length];
            WITH_CHECK_ERROR(mips_mem_read(handler, addr, length, buffer));
            swap_endianness(buffer, length);
            memcpy(data_out, buffer, length * sizeof(uint8_t));
            delete[] buffer;
        }

        void write(
            mips_mem_h handler,
            addr_t addr,
            addr_t length,
            const mem_data_t * data_in
        ) {
            uint8_t * buffer = new uint8_t[length];
            memcpy(buffer, data_in, length * sizeof(mem_data_t));
            swap_endianness(buffer, length);
            WITH_CHECK_ERROR(mips_mem_write(handler, addr, length, buffer));
            delete[] buffer;
        }

        void swap_endianness(uint8_t * ptr, int length = 4) {
            for (int i = 0 ; i < length / 4 ; i++) {
                std::swap(ptr[4 * i + 0], ptr[4 * i + 3]);
                std::swap(ptr[4 * i + 1], ptr[4 * i + 2]);
            }
        }
    }
}
