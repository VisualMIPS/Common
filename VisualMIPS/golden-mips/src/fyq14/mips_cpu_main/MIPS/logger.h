#ifndef MIPS_LOGGER_H
#define MIPS_LOGGER_H

#include "types.h"

#include <stdio.h>
#include <stdarg.h>

namespace MIPS {
    class Logger {
    private:
        FILE * fout;
        debug_level_t level;
    public:
        const static debug_level_t EMERGENCY = 0;
        const static debug_level_t WARNING = 1;
        const static debug_level_t DEBUG = 2;

        // Constructors
        Logger();
        Logger(debug_level_t dest);
        Logger(FILE * dest);
        Logger(FILE * dest, debug_level_t);

        ~Logger();
        void set_level(debug_level_t);
        void set_fout(FILE *);

        // logging functions
        void error(const char *);
        void warning(const char *);
        void debug(const char *, ...);
    };
}

#endif
