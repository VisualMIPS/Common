namespace VisualMIPS

module Rtypes = 
    open Types
    open Instructions
    open MachineState
    
    //simpleR functions

    //bear in mind that when a user writes a negative number, 
    // it is stored as a 2's complement in the uint32 type

    let ADDU (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        Word (rS + rT)

    let opAND (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        Word (rS &&& rT)

    let opOR (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        Word (rS ||| rT)

    let opSRAV (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        // signed : duplicating the sign-bit (bit 31) in the emptied bits
        // converting rT to int makes the bitwise operator treat it as a signed nbr
        Word ( uint32(int32 rT >>> int32 rS) )

    let opSRLV (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        // unsigned : inserting zeros into the emptied bits
        // rT is uint so it will put 0 automatically
        Word (rT >>> int32 rS)

    let opSLLV (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        // inserting zeros into the emptied bits
        Word (rT <<< int32 rS)
    
    let opSUBU (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        Word (rS - rT)

    let opXOR (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        Word (rS ^^^ rT)

    let opSLT (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        //signed comparison -> convert as signed nbr to treat them as so
        match int32(rS) < int32(rT) with
        | true -> Word(uint32 1)
        | false -> Word(uint32 0)
    
    let opSLTU (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        //unsigned comparison, no pre-processing on rT or rS
        match rS < rT with
        | true -> Word(uint32 1)
        | false -> Word(uint32 0)

    //shiftR functions

    let opSRA (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) (Shiftval shiftval) =
        failwithf "not implemented yet"