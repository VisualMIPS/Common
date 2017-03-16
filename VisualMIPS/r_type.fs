namespace VisualMIPS

module Rtypes = 
    open Types
    open Instructions
    open MachineState
    

    //bear in mind that when a user writes a negative number, 
    // it is stored as a 2's complement in the uint32 type

     //fullR functions

    let opADD (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        let (+.) x y = Checked.(+) x y
        try
            let output = Word ( uint32( int32(rS) +. int32(rT) ) )
            (output, mach)
        with e -> //overflow occured
            let outputSameRd = getReg instr.rd mach
            let newmach = {mach with State = RunTimeErr "Overflow on ADD"}
            (outputSameRd , newmach)

    let opSUB (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        let (-.) x y = Checked.(-) x y
        try
            let output = Word ( uint32( int32(rS) -. int32(rT) ) )
            (output, mach)
        with e -> 
            let outputSameRd = getReg instr.rd mach
            let newmach = setState (RunTimeErr "Overflow on SUB") mach
            (outputSameRd , newmach)


// left to do : DIV | DIVU | MULT | MULTU | JR | JALR | MTHI | MTHLO

    let opDIV (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        match rT with
        | 0u -> mach //div by 0 -> nothing happens
        | _ ->
            
            let newmach = mach |> setHi word |> setLo word 
       