#include "MIPS/exception.h"

namespace MIPS {
    Exception::Exception() {}

    Exception::Exception(std::string s, mips_error error_code) {
        metadata = s;
        errcode = error_code;
    }

    std::string Exception::to_str() const {
        return metadata;
    }

    std::string Exception::get_metadata() const {
        return metadata;
    }
      
    mips_error Exception::get_error_code() const {
        return errcode;
    }

#define DEFEXCEPTION(name, code) \
    MIPS::Exception name() { \
      return MIPS::Exception(std::string(#name), code); \
    }

    DEFEXCEPTION(NotImplementedException, mips_ErrorNotImplemented)
    DEFEXCEPTION(InvalidArgumentException, mips_ErrorInvalidArgument)
    DEFEXCEPTION(InvalidHandleErrorException, mips_ErrorInvalidHandle)
    DEFEXCEPTION(FileReadErrorException, mips_ErrorFileReadError)
    DEFEXCEPTION(FileWriteErrorException, mips_ErrorFileWriteError)
    DEFEXCEPTION(BreakException, mips_ExceptionBreak)
    DEFEXCEPTION(InvalidAddressException, mips_ExceptionInvalidAddress)
    DEFEXCEPTION(InvalidAlignmentException, mips_ExceptionInvalidAlignment) 
    DEFEXCEPTION(AccessViolationException, mips_ExceptionAccessViolation)
    DEFEXCEPTION(InvalidInstructionException, mips_ExceptionInvalidInstruction)
    DEFEXCEPTION(ArithmeticOverflowException, mips_ExceptionArithmeticOverflow)
    DEFEXCEPTION(InternalErrorException, mips_InternalError)
    DEFEXCEPTION(UnkownErrorException, mips_InternalError)

#undef DEFEXCEPTION
}

std::ostream &operator<<(std::ostream &os, MIPS::Exception const &ex) {
    os << "MIPS::Exception : " <<
        ex.to_str() << " " <<
        ex.get_metadata();
    return os;
}

