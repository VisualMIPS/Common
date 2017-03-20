#ifndef MIPS_MEMORY_H
#define MIPS_MEMORY_H

#include <algorithm>
#include <cstring>

#include "mips.h"
#include "types.h"
#include "exception.h"

namespace MIPS {
    namespace memory {
        void read(mips_mem_h handler, addr_t, addr_t, mem_data_t*);
        void write(mips_mem_h handler, addr_t, addr_t, const mem_data_t*);
        void swap_endianness(mem_data_t*, int);
    }
}

#endif
