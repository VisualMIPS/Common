namespace VisualMIPS

module Itypes = 
    open Types
    open Instructions
    open MachineState


    //fullI functions

    let opADDI (mach: MachineState) (instr : Instruction) (Word rS) (Half immediate) =
        let output32 =  int64( int32(rS) + int32(immediate) )
        let output64 =  int64(rS) + int64(immediate)
        match output32=output64 with
        | true -> 
            let output = Word( rS + uint32(immediate) )
            (output, mach)
        | false -> //overflow occured
            let outputSameRd = getReg instr.rd mach
            let newmach = setState (RunTimeErr "Overflow on ADD") mach
            (outputSameRd , newmach)