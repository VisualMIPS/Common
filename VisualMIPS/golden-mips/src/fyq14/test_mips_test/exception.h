#ifndef TEST_EXCEPTION_H
#define TEST_EXCEPTION_H

#include <exception>
#include <iostream>
#include <string>
#include <sstream>

namespace test {

    class Exception : public std::exception {
    private:
        std::string reason;
    public:
        Exception(const std::string &);
        Exception(const char *);
        std::string to_string();
        void croak();
    };

    Exception FailedTestException();
    Exception BadTestCaseException();
}

#endif
