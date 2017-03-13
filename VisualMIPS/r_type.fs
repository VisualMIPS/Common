namespace VisualMIPS

module Rtypes = 
    open Types
    open Instructions
    open MachineState
    
    let opAND (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        Word (rS &&& rT)

    let opOR (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        Word (rS ||| rT)

    let opXOR (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        Word (rS ^^^ rT)

    let opSRA (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) (Shiftval shiftval)=
        Word (rS >>> int32 shiftval)

