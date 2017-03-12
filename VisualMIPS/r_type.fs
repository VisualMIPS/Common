namespace VisualMIPS

module Stuff = 
    open Types
    open Instructions
    open MachineState
    
    let opAND (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        setReg mach instr.rd (Word (rS &&& rT))

    let opOR (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        setReg mach instr.rd (Word (rS ||| rT))

    let opSRA (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        setReg mach instr.rd (Word (rS >>> int32 (T.getValue instr.shift)))

    let opXOR (mach: MachineState) (instr : Instruction) (Word rS) (Word rT) =
        setReg mach instr.rd (Word (rS ^^^ rT))
