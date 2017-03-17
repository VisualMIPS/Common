namespace VisualMIPS

module Itypes = 
    open Types
    open Instructions
    open MachineState


    //fullI functions

    let opADDI (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        let immediate = int32(T.getValue instr.immed)
        let (+.) x y = Checked.(+) x y
        try
            let out = Word( uint32( int32(rS) +. immediate ))
            (out, mach)
        with e ->
            let outputSameRt = getReg instr.rt mach
            let chgmach = setState (RunTimeErr "Overflow on ADDI") mach
            (outputSameRt , chgmach)