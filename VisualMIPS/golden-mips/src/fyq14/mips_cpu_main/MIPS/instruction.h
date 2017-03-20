#ifndef MIPS_INSTRUCTION_H
#define MIPS_INSTRUCTION_H

#include <queue>
#include <utility>

#include "register.h"
#include "types.h"

namespace MIPS {
    class Instruction {
    public:
        virtual ~Instruction();
        void virtual execute() = 0;
    };

    // ========================================
    // The InvalidInstruction, simply throws an exception
    // ========================================

    class InvalidInstruction : public Instruction {
    public:
        void execute();
    };

    // ========================================
    // RInstruction
    // ========================================

    class RInstruction : public Instruction {
    protected:
        // pointer to a function to be run to compute fnc(rs->get(), rt->get(), *rd)
        // should return boolean, true if it overflows, false otherwise
        operator_t fnc;
        // std::function<uint32_t(uint32_t, uint32_t, uint32_t*)> fnc; 
        RegisterInterface * rs;
        RegisterInterface * rt;
        RegisterInterface * rd;
        unsigned shift;
    public:
        void set_rs(RegisterInterface *);
        void set_rt(RegisterInterface *);
        void set_rd(RegisterInterface *);
    };

    class ArithmeticRInstruction : public RInstruction {
    private:
        bool is_can_overflow;
    public:
        ArithmeticRInstruction(operator_t, bool);
        void virtual execute();
    };

    class ShiftRInstruction : public RInstruction {
    private:
        uint32_t shift;
        shift_fnc_t fnc;
    public:
        ShiftRInstruction(shift_fnc_t);
        void set_shift(uint32_t);
        void execute();
    };

    class MultDivInstruction : public RInstruction {
    private:
        RegisterInterface * ptr_r_lo;
        RegisterInterface * ptr_r_hi;
        operator_t fnc_lo;
        operator_t fnc_hi;
    public:
        MultDivInstruction(operator_t, operator_t);
        void set_r_lo(RegisterInterface *);
        void set_r_hi(RegisterInterface *);
        void virtual execute();
    };

    class JumpRInstruction : public RInstruction {
    protected:
        std::queue<std::pair<uint32_t, MIPS::jump_t>> * ptr_delayed_branches;
    public:
        JumpRInstruction(std::queue<std::pair<uint32_t, MIPS::jump_t>> *);
        void virtual execute();
    };

    class JumpLinkRInstruction : public JumpRInstruction {
        RegisterInterface * ptr_pc;
    public:
        using JumpRInstruction::JumpRInstruction;
        void execute();
        void set_pc(RegisterInterface *);
    };

    // ========================================
    // IInstruction
    // ========================================

    class IInstruction : public Instruction {
    protected:
        RegisterInterface * ptr_rs;
        RegisterInterface * ptr_rt;
        uint32_t konst;
    public:
        IInstruction();
        void set_rs(RegisterInterface*);
        void set_rt(RegisterInterface*);
        void set_konst(uint32_t);
    };

    class BitwiseIInstruction : public IInstruction {
    private:
        operator_t fnc;
    public:
        BitwiseIInstruction(operator_t);
        void execute();
    };

    class ArithmeticIInstruction : public IInstruction {
    private:
        bool is_can_overflow;
        operator_t fnc;
    public:
        ArithmeticIInstruction(operator_t, bool);
        void execute();
    };

    template <bool is_write, uint32_t n_bytes, bool is_signed>
    class MemoryReadWriteIInstruction : public IInstruction {
    protected:
        mips_mem_h ptr_mem;
        uint32_t get_addr();
        uint32_t sign_extend(uint32_t arg);
        uint32_t get_mask();
    public:
        MemoryReadWriteIInstruction(mips_mem_h);
        virtual void execute();
    };

    class LWLIInstruction : public MemoryReadWriteIInstruction<false, 2, false> {
    public:
        using MemoryReadWriteIInstruction::MemoryReadWriteIInstruction;
        void execute();
    };

    class LWRIInstruction : public MemoryReadWriteIInstruction<false, 2, false> {
    public:
        using MemoryReadWriteIInstruction::MemoryReadWriteIInstruction;
        void execute();
    };

    // Transform the provided constant into something cool using a transformation function
    // that does absolutely nothing
    class LoadIInstruction : public IInstruction {
    private:
        transform_t fnc;
    public:
        LoadIInstruction(transform_t);
        void execute();
    };

    class BranchIInstruction : public IInstruction {
    protected:
        comparator_t fnc;
        std::queue<std::pair<uint32_t, MIPS::jump_t>> * ptr_delayed_branches;
    public:
        BranchIInstruction(
            comparator_t,
            std::queue<std::pair<uint32_t, MIPS::jump_t>> *
        );
        void virtual execute();
    };

    class BranchLinkIInstruction : public BranchIInstruction {
    protected:
        RegisterInterface * ptr_r_return;
        RegisterInterface * ptr_pc;
    public:
        using BranchIInstruction::BranchIInstruction;
        void set_pc(RegisterInterface*);
        void set_r_return(RegisterInterface*);
        void execute();
    };

    // ========================================
    // JInstruction
    // ========================================

    class JInstruction : public Instruction {
    protected:
        uint32_t addr;
    public:
        void set_addr(uint32_t);
    };

    class JumpJInstruction : public JInstruction {
    protected:
        std::queue<std::pair<uint32_t, MIPS::jump_t>> * ptr_delayed_branches;
    public:
        JumpJInstruction(std::queue<std::pair<uint32_t, MIPS::jump_t>> *);
        void virtual execute();
    };

    class JumpLinkJInstruction : public JumpJInstruction {
    private:
        RegisterInterface * ptr_r_return;
        RegisterInterface * ptr_pc;
    public:
        using JumpJInstruction::JumpJInstruction;
        void set_ptr_r_return(RegisterInterface*);
        void set_ptr_pc(RegisterInterface*);
        void execute();
    };
}

#endif
