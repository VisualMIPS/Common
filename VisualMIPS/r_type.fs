namespace VisualMIPS

module Rtypes = 
    open Types
    open Instructions
    open MachineState
    

    //bear in mind that when a user writes a negative number, 
    // it is stored as a 2's complement in the uint32 type

     //fullR functions

     //fullR functions

    let opADD (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        let output32 =  int64( int32(rS) + int32(rT) )
        let output64 =  int64( int32( rS)) + int64(rT)
        match output32=output64 with
        | true -> 
            let output = Word( rS + rT )
            (output, mach)
        | false -> //overflow occured
            let outputSameRd = getReg instr.rd mach
            let newMach = setState (RunTimeErr "Overflow on ADD") mach
            (outputSameRd , newMach)
    
    let opSUB (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        let output32 =  int64( int32(rS) - int32(rT) )
        let output64 =  int64( int32( rS)) - int64(rT)
        match output32=output64 with
        | true -> 
            let output = Word( rS - rT )
            (output, mach)
        | false -> //overflow occured
            let outputSameRd = getReg instr.rd mach
            let newMach = setState (RunTimeErr "Overflow on SUB") mach
            (outputSameRd , newMach)


// left to do :  JR | JALR | 

    let opDIV (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        match rT with
        | 0u -> mach //div by 0 -> nothing happens
        | _ -> // rS = q*rT + r 
            try
                let quotient = Word(uint32( int32(rS)/int32(rT) ))
                let remainder = Word(uint32( int32(rS)%int32(rT) ))
                let newMach = mach |> setHi remainder |> setLo quotient 
                newMach
            with e->
                mach
        
    let opDIVU (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        match rT with
        | 0u -> mach 
        | _ -> 
            let quotient = Word( rS/rT )
            let remainder = Word( rS%rT )
            let newMach = mach |> setHi remainder |> setLo quotient 
            newMach

    let opMULT (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
    // no overflow possible as long as Ben checks range in parser
        let result = uint64( int64( int32( rS )) * int64( int32 ( rT ))) //may be able to simplify here
        let upper = Word( uint32( result >>> 32 )) 
        let lower = Word( uint32( result )) //selects the first 32 btis
        let newMach = mach |> setHi upper |> setLo lower 
        newMach

    let opMULTU (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        let result = uint64(rS) * uint64(rT) 
        let upper = Word( uint32( result >>> 32 )) 
        let lower = Word( uint32( result )) //selects the first 32 btis
        let newMach = mach |> setHi upper |> setLo lower 
        newMach

    let opJR (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        let returnMach =
            match ((rS &&& 3u) = 0u) with
            | true ->
                setNextNextPC (Word(rS)) mach
            | false -> 
                setState (RunTimeErr "Address Error on JR") mach
        returnMach
        // execute isntruction following the jump in the branch delay slot before jumping
       

    let opJALR (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        // returns output = return_addr & mach with rs in PC
        let returnAddress = Word( T.getValue(getNextPC mach) + 8u ) 
        let returnMach =
            match ((rS &&& 3u) = 0u) with
            | true ->
                setNextNextPC (Word(rS)) mach
            | false -> 
                setState (RunTimeErr "Address Error on JALR") mach
        (returnAddress, returnMach)

    let opMTHI (mach: MachineState) (instr : Instruction) (rS) (Word rT) =
        setHi rS mach
    
    let opMTLO (mach: MachineState) (instr : Instruction) (rS) (Word rT) =
        setLo rS mach
    
