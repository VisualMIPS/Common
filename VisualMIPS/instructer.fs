//Decode/depatch
namespace VisualMIPS

module Executor =
    open Types
    open Instructions
    open MachineState
    open Rtypes

    let processMultDiv (mach : MachineState) (instr: Instruction) = failwith "Not Implemented"

    let processHILO (mach : MachineState) (instr: Instruction) = failwith "Not Implemented"

    let processSimpleR (mach : MachineState) (instr: Instruction) =
        let localMap = Map [(AND,opAND);(OR, opOR); (XOR, opXOR)]
        let rs = getReg mach instr.rs
        let rt = getReg mach instr.rt
        let fn = Map.find instr.opcode localMap
        let output = fn mach instr rs rt
        setReg mach instr.rd output
    
    let processShiftR (mach : MachineState) (instr : Instruction) =
        let localMap = Map [(SRA, opSRA)]
        let rs = getReg mach instr.rs
        let rt = getReg mach instr.rt
        let fn = Map.find instr.opcode localMap
        let output = fn mach instr rs rt instr.shift
        setReg mach instr.rd output

    let opTypeMap = Map [([DIV; DIVU; MULT; MULTU],processMultDiv);
                        ([MFHI; MFLO; MTHI; MTLO],processHILO);
                        ([AND; XOR; OR],processSimpleR);
                        ([SRA],processShiftR)
                        ]

    let executeInstruction (mach : MachineState) (instr : Instruction) =
        let key = Map.findKey (fun x _ -> List.contains instr.opcode x) opTypeMap //Could be slightly simpler by addition
        let fn = Map.find key opTypeMap
        fn mach instr
