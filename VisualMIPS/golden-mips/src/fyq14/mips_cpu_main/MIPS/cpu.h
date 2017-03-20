#ifndef MIPS_CPU_H
#define MIPS_CPU_H

#include <utility>
#include <queue>

#include "logger.h"
#include "memory.h"
#include "register.h"
#include "types.h"
#include "instruction_decoder.h"
#include "instruction_fetcher.h"

struct mips_cpu_impl {
    friend class MIPS::InstructionDecoder;
private:
    MIPS::InstructionFetcher fetcher;
    MIPS::InstructionDecoder decoder;
    MIPS::RegisterInterface * ptr_pc;
    MIPS::RegisterInterface * ptr_registers[32];
    MIPS::RegisterInterface * ptr_register_lo;
    MIPS::RegisterInterface * ptr_register_hi;
    mips_mem_h mem_handler;
    // delayed_branches::second  simply indicates whether it's relative or absolute.
    // true is relative, false if absolute
    std::queue<std::pair<uint32_t, MIPS::jump_t>> delayed_branches;
    void jump(addr_t addr);
public:
    mips_cpu_impl(mips_mem_h);
    ~mips_cpu_impl();
    uint32_t get_pc();
    void set_pc(uint32_t);
    uint32_t get_register(unsigned);
    void set_register(unsigned, uint32_t);
    void reset();
    void step();
    MIPS::Logger logger;
};

#endif
