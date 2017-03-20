#ifndef MIPS_INSTRUCTION_FETCHER_H
#define MIPS_INSTRUCTION_FETCHER_H

#include <queue>
#include "mips_core.h"
#include "types.h"
#include "memory.h"

namespace MIPS {
    class InstructionFetcher {
        private:
            mips_cpu_impl * ptr_cpu;
            mips_mem_h mem_handler;
            addr_t head;
            std::queue<instruction_t> instruction_queue;
        public:
            InstructionFetcher(mips_mem_h, mips_cpu_impl*);
            InstructionFetcher(mips_mem_h);
            void set_address(instruction_t);
            void fetch();
            void clear();
            void jump(addr_t);
            uint32_t buffer_size() const;
            addr_t get_head() const;
        friend InstructionFetcher& operator>>(InstructionFetcher &, instruction_t &);
    };
    
}

#endif
