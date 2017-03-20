#ifndef MIPS_REGISTER_H
#define MIPS_REGISTER_H

#include "exception.h"
#include "types.h"

namespace MIPS {
    class RegisterInterface {
    protected:
        mips_cpu_impl * ptr_cpu;
    public:
        RegisterInterface();
        virtual ~RegisterInterface();
        virtual uint32_t get() const = 0;
        virtual void set(uint32_t) = 0;
        void set_ptr_cpu(mips_cpu_impl *);
    };

    class RegularRegister : public RegisterInterface  {
    private:
        uint32_t value;
        int id;
    public:
        static const int ID_LO = 32;
        static const int ID_HI = 33;
        static const int ID_PC = 34;
        RegularRegister();
        RegularRegister(uint32_t);
        uint32_t get() const;
        void set(uint32_t);
        void set_index(int);
    };

    class ZeroRegister : public RegisterInterface {
    public:
        ZeroRegister();
        ZeroRegister(uint32_t);
        uint32_t get() const;
        void set(uint32_t);
    };
}

#endif
