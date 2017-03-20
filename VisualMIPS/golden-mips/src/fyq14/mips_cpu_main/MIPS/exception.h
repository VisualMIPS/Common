#ifndef MIPS_EXCEPTION_H
#define MIPS_EXCEPTION_H

// STL
#include <string>
#include <ostream>
#include <exception>

// Our libs
#include "mips_core.h"

#define WITH_CHECK_ERROR(statement) \
{\
    mips_error err = (statement); \
    switch(err) { \
        case mips_Success: \
            break; \
        case mips_ErrorNotImplemented: \
            throw MIPS::NotImplementedException(); \
        case mips_ErrorInvalidArgument: \
            throw MIPS::InvalidArgumentException(); \
        case mips_ErrorInvalidHandle: \
            throw MIPS::InvalidHandleErrorException(); \
        case mips_ErrorFileReadError: \
            throw MIPS::FileReadErrorException(); \
        case mips_ErrorFileWriteError: \
            throw MIPS::FileWriteErrorException(); \
        case mips_ExceptionBreak: \
            throw MIPS::BreakException(); \
        case mips_ExceptionInvalidAddress: \
            throw MIPS::InvalidAddressException(); \
        case mips_ExceptionInvalidAlignment: \
            throw MIPS::InvalidAlignmentException(); \
        case mips_ExceptionInvalidInstruction: \
            throw MIPS::InvalidInstructionException(); \
        case mips_ExceptionArithmeticOverflow: \
            throw MIPS::ArithmeticOverflowException(); \
        case mips_InternalError: \
            throw MIPS::InternalErrorException(); \
        default: \
            throw MIPS::UnkownErrorException(); \
    } \
}

namespace MIPS {
    class Exception : public std::exception {
    private:
        std::string metadata;
	mips_error errcode;
    public:
        Exception();
        Exception(std::string s, mips_error error_code);
        std::string get_metadata() const;
        std::string to_str() const;
        mips_error get_error_code() const;
    };

#define DEFEXCEPTION(name) Exception name();

    DEFEXCEPTION(NotImplementedException)
    DEFEXCEPTION(InvalidArgumentException)
    DEFEXCEPTION(InvalidHandleErrorException)
    DEFEXCEPTION(FileReadErrorException)
    DEFEXCEPTION(FileWriteErrorException)
    DEFEXCEPTION(BreakException)
    DEFEXCEPTION(InvalidAddressException)
    DEFEXCEPTION(InvalidAlignmentException)
    DEFEXCEPTION(AccessViolationException)
    DEFEXCEPTION(InvalidInstructionException)
    DEFEXCEPTION(ArithmeticOverflowException)
    DEFEXCEPTION(InternalErrorException)
    DEFEXCEPTION(UnkownErrorException)

#undef DEFEXCEPTION

}

#endif
