# Your login. For example, mine is dt10. Yours
# won't be eie2ugs...
LOGIN ?= fyq14
.DEFAULT_GOAL := run

CPPFLAGS += -W -Wall -g
CPPFLAGS += -I include

CFLAGS	 += -std=c99
CXXFLAGS += -std=c++11

# Force the inclusion of C++ standard libraries
LDLIBS += -lstdc++

DEFAULT_OBJECTS = \
    src/shared/mips_test_framework.o \
    src/shared/mips_mem_ram.o 

USER_CPU_SRCS = \
    $(wildcard src/$(LOGIN)/mips_cpu.c) \
    $(wildcard src/$(LOGIN)/mips_cpu.cpp) \
    $(wildcard src/$(LOGIN)/mips_cpu_*.c) \
    $(wildcard src/$(LOGIN)/mips_cpu_*.cpp) \
    $(wildcard src/$(LOGIN)/mips_cpu_main/mips_cpu_*.c) \
    $(wildcard src/$(LOGIN)/mips_cpu_main/mips_cpu_*.cpp)

TEST_SRCS= \
    $(wildcard src/$(LOGIN)/test_mips.c) \
    $(wildcard src/$(LOGIN)/test_mips.cpp) \
    $(wildcard src/$(LOGIN)/test_mips_*.c) \
    $(wildcard src/$(LOGIN)/test_mips_*.cpp) \
    $(wildcard src/$(LOGIN)/test_mips_*/test_mips_*.c) \
    $(wildcard src/$(LOGIN)/test_mips_*/test_mips_*.cpp) 

USER_CPU_OBJECTS = $(patsubst %.c,%.o,$(patsubst %.cpp,%.o,$(USER_CPU_SRCS)))
TEST_OBJECTS = $(patsubst %.c,%.o,$(patsubst %.cpp,%.o,$(TEST_SRCS)))
UNIT_TEST_OBJECTS = $(patsubst %.c,%.o,$(patsubst %.cpp,%.o,$(UNIT_TEST_SRCS)))

src/$(LOGIN)/unit_test_mips: $(DEFAULT_OBJECTS) $(USER_CPU_OBJECTS) $(UNIT_TEST_OBJECTS)
src/$(LOGIN)/test_mips : $(DEFAULT_OBJECTS) $(USER_CPU_OBJECTS) $(TEST_OBJECTS)

fragments/run_fibonacci : $(DEFAULT_OBJECTS) $(USER_CPU_OBJECTS)
    
fragments/run_addu : $(DEFAULT_OBJECTS) $(USER_CPU_OBJECTS)

shout:
	echo $(TEST_OBJECTS) $(USER_CPU_OBJECTS)

# Clean all object files (and perhaps executables?)
clean:
	find . -name "*.o" -print0 | xargs -0 rm -rf

# Runs the contracted test suite
run: src/$(LOGIN)/test_mips
	cd src/$(LOGIN) && ./test_mips

# Runs the unit test, specific to my implementation (supports C++ only!)
# Has exposure to the internal classes
test: src/$(LOGIN)/unit_test_mips
	./$<

