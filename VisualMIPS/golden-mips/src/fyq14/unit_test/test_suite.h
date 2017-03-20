// A little working test suite
#include <stdlib.h>
#include <stdio.h>
#include <time.h>

#define DEFTEST(name) \
    void _test_##name(bool * _flag, unsigned * _assertions_count, unsigned * _line)

#define ASSERT(condition) \
    if (!(condition)) { \
        *_flag = false; \
        *_line = __LINE__; \
        return; \
    } else { \
        *_assertions_count = *_assertions_count + 1; \
    }

#define RUNTEST(NAME) \
    { \
        srand(time(NULL)); \
        bool flag = true; \
        unsigned line; \
        unsigned assertions_count = 0; \
        puts("======================="); \
        puts("Test " #NAME); \
        _test_##NAME(&flag, &assertions_count, &line); \
\
        printf("%u assertion(s) ", assertions_count); \
        if(flag) { \
            printf("\n"); \
        } else { \
            printf("with a failure at line %u\n", line); \
        } \
        puts("=======================\n"); \
    }

