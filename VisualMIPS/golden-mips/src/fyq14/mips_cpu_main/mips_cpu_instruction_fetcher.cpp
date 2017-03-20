#include "MIPS/instruction_fetcher.h"
#include "MIPS/cpu.h"

namespace MIPS {
    InstructionFetcher::InstructionFetcher(mips_mem_h arg) : ptr_cpu(NULL) {
        mem_handler = arg; 
    }

    InstructionFetcher::InstructionFetcher(mips_mem_h arg, mips_cpu_impl * ptr_cpu_in) {
        mem_handler = arg; 
        ptr_cpu = ptr_cpu_in;
    }

    void InstructionFetcher::set_address(addr_t addr) {
        head = addr;
    }

    void InstructionFetcher::fetch() {
        // TODO : Memeory handle memory read error?
        uint8_t buffer[4];
        if (ptr_cpu != NULL) {
            ptr_cpu->logger.debug("    Fetching from address 0x%.8X\n", head);
        }
        memory::read(mem_handler, head, 4, buffer);

        if (ptr_cpu != NULL) {
            ptr_cpu->logger.debug("    => FETCHED! vector = 0x%.8X\n", *((uint32_t*) buffer));
        }

        // Assume that host machine is little endian!
        // TODO : Host machine endianess check?
        instruction_queue.push(
            *((uint32_t*) buffer) 
        );
        if (ptr_cpu != NULL) {
            ptr_cpu->logger.debug("    => Advancing head from  0x%.8X to 0x%.8x\n",
                head, head + 4);
        }
        head = head + 4;
    }

    void InstructionFetcher::clear() {
        while(!instruction_queue.empty()) {
            instruction_queue.pop();
        }
    }

    void InstructionFetcher::jump(addr_t new_head) {
        clear();
        head = new_head;
    }

    uint32_t InstructionFetcher::buffer_size() const {
        return instruction_queue.size();
    }

    addr_t InstructionFetcher::get_head() const {
        return head;
    }

    InstructionFetcher& operator>>(
        InstructionFetcher & fetcher,
        instruction_t & dest
    ) {
        if (fetcher.instruction_queue.empty()) {
            fetcher.fetch();
        }

        dest = fetcher.instruction_queue.front();
        fetcher.instruction_queue.pop();
        return fetcher;
    }
}
