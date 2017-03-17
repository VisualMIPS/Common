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
            let newmach = setState (RunTimeErr "Overflow on ADD") mach
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


// left to do :  JR | JALR | 

    let opDIV (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        match rT with
        | 0u -> mach //div by 0 -> nothing happens
        | _ -> // rS = q*rT + r 
            let quotient = Word(uint32( int32(rS)/int32(rT) ))
            let remainder = Word(uint32( int32(rS)%int32(rT) ))
            let newmach = mach |> setHi remainder |> setLo quotient 
            newmach
        
    let opDIVU (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        match rT with
        | 0u -> mach 
        | _ -> 
            let quotient = Word( rS/rT )
            let remainder = Word( rS%rT )
            let newmach = mach |> setHi remainder |> setLo quotient 
            newmach

    let opMULT (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
    // no overflow possible as long as Ben checks range in parser
        let result = uint64( int64( int32( rS )) * int64( int32 ( rT ))) //may be able to simplify here
        let upper = Word( uint32( result >>> 32 )) 
        let lower = Word( uint32( result )) //selects the first 32 btis
        let newmach = mach |> setHi upper |> setLo lower 
        newmach

    let opMULTU (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        let result = uint64(rS) * uint64(rT) 
        let upper = Word( uint32( result >>> 32 )) 
        let lower = Word( uint32( result )) //selects the first 32 btis
        let newmach = mach |> setHi upper |> setLo lower 
        newmach

    let opJR (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        failwithf "Not implemented yet"

    let opJALR (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        failwithf "Not implemented yet"

    let opMTHI (mach: MachineState) (instr : Instruction) (rS) (Word rT) =
        setHi rS mach
    
    let opMTLO (mach: MachineState) (instr : Instruction) (rS) (Word rT) =
        setLo rS mach
    
