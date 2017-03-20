#include <stdio.h>

#include "mips_core.h"
#include "mips_test.h"

#include "test_mips_test/test.h"
using namespace std;

int main() {    
    mips_mem_h mem=mips_mem_create_ram(4096, 4);
    
    mips_cpu_h cpu=mips_cpu_create(mem);
    
    mips_error e=mips_cpu_set_debug_level(cpu, 4, stderr);
    if(e!=mips_Success){
        fprintf(stderr, "mips_cpu_set_debug_level : failed.\n");
        exit(1);
    }
    
    //8722465 <-- add r4 and r5
    int loop = 1;
    while (loop) {
        char i;
        cin >> i;
        switch (i) {
            case 's':
            {
                for (int i = 0; i <32; i++) {
                    uint32_t got;
                    e=mips_cpu_get_register(cpu, i, &got);
                    cout << i << " " << got << endl;
                }
                //cout << "Printing State" << endl;
                break;
            }
            case 'm':
            {
                //set a word of memory
                uint32_t addr;
                uint32_t val;
                cin >> addr >> val;
                cout << "Setting " << addr << " to " << val << endl;
                break;
            }
            case 'i':
            {
                mips_cpu_set_pc(cpu,0x00);
                
                uint32_t instr;
                cin >> instr;
                //cout << "Running instruction " << instr << endl;
                
                uint8_t buffer[4];
                buffer[0]=(instr>>24)&0xFF;
                buffer[1]=(instr>>16)&0xFF;
                buffer[2]=(instr>>8)&0xFF;
                buffer[3]=(instr>>0)&0xFF;
                //Instruction always at pos 0
                e=mips_mem_write(
                    mem,	        //!< Handle to target memory
                    0,	            //!< Byte address to start transaction at
                    4,	            //!< Number of bytes to transfer
                    buffer	        //!< Receives the target bytes
                );
                e=mips_cpu_step(cpu);
                if(e!=mips_Success){
                    fprintf(stderr, "mips_cpu_step : failed.\n");
                    exit(1);
                }
                break;
            }
            case 'r':
            {
                int regnum;
                uint32_t regval;
                cin >> regnum >> regval;
                //cout << "Setting register " << regnum << " to " << regval << endl;
                e=mips_cpu_set_register(cpu, regnum, regval);
                break;
            }
            case 'c':
            {
                cout << "Clearing everything" << endl;
                break;
            }
            case 'q':
                loop = 0;
                break;
            default:
                cout << "Unknown" << endl;
        }
        
    }    
    /*
    uint32_t instr =
        (0ul << 26) // opcode = 0
        |
        (4ul << 21) // srca = r4
        |
        (5ul << 16) // srcb = r5
        |
        (3ul << 11) // dst = r3
        |
        (0ul << 6) // shift = 0
        |
        (0x21 << 0);
    cout << instr << endl;
    uint8_t buffer[4];
    buffer[0]=(instr>>24)&0xFF;
    buffer[1]=(instr>>16)&0xFF;
    buffer[2]=(instr>>8)&0xFF;
    buffer[3]=(instr>>0)&0xFF;
    //Instruction always at pos 0
    e=mips_mem_write(
        mem,	        //!< Handle to target memory
        0,	            //!< Byte address to start transaction at
        4,	            //!< Number of bytes to transfer
        buffer	        //!< Receives the target bytes
    );
    if(e!=mips_Success){
        fprintf(stderr, "mips_mem_write : failed.\n");
        exit(1);
    }
    
    // 2 - put register values in cpu
    e=mips_cpu_set_register(cpu, 4, 40);
    e=mips_cpu_set_register(cpu, 5, 50);
    
    // 3 - step CPU
    e=mips_cpu_step(cpu);
    if(e!=mips_Success){
        fprintf(stderr, "mips_cpu_step : failed.\n");
        exit(1);
    }
    
    // 4 -Check the result
    uint32_t got;
    e=mips_cpu_get_register(cpu, 3, &got);
    
   
    int passed = got == 40+50;
    
    
    cout << got << endl;
    */
}
