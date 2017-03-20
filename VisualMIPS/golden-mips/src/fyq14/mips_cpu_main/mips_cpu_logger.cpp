#include "MIPS/logger.h"

namespace MIPS {
    Logger::Logger() {
        level = EMERGENCY;
        fout = stdout;
    }

    Logger::Logger(debug_level_t level_in) {
        level = level_in;
        fout = stdout;
    }

    Logger::Logger(FILE * dest) {
        level = EMERGENCY;
        fout = dest;
    }

    Logger::Logger(FILE * dest, debug_level_t level_in) {
        level = level_in;
        fout = dest;
    }

    Logger::~Logger() {
        if (fout != NULL &&
            fout != stdout &&
            fout != stderr) {
            fclose(fout);
        }
        // Defensive programming ...
        fout = NULL;
    }

    void Logger::set_level(debug_level_t level_in) {
        level = level_in;
    }

    void Logger::set_fout(FILE * fout_in) {
        if (fout != fout_in &&
            fout != stdout &&
            fout != stderr &&
            fout != NULL) {
            // close fout
            fclose(fout);
        }

        fout = fout_in;
    }

    void Logger::error(const char * x) {
        fputs(x, fout);
    }

    void Logger::warning(const char * x) {
        switch (level) {
        case DEBUG:
        case WARNING:
            fputs(x, fout);
        }
    }

    void Logger::debug(const char * format, ...) {
        switch (level) {
            case DEBUG:
                 va_list args;
                 va_start(args, format);
                 vfprintf(fout, format, args);
                 va_end(args);
                 fflush(fout);
           }
    }
}

