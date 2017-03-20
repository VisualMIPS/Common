#include "exception.h"


namespace test {
    Exception::Exception(const std::string & in) :
        reason(in) {}

    Exception::Exception(const char * in) :
        reason(in) {}

    std::string Exception::to_string() {
        std::stringstream ss;
        ss << reason << " : "
            << (reason.length() > 1 ? reason : "No reason provided")
            << "\n";
        return ss.str();
    }

    void Exception::croak() {
        std::cerr << to_string() << std::endl;
    }

    // Subclassses

    Exception FailedTestException() {
        return Exception("FailedTestException");
    }

    Exception BadTestCaseException() {
        return Exception("BadtestCaseException");
    }

}
