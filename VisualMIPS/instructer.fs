//Decode/depatch
namespace VisualMIPS

module Executor =
    open Types
    open Instructions
    open MachineState
    open Rtypes

    let processMultDiv (instr: Instruction)  (mach : MachineState)= failwith "Not Implemented"

    let processHILO (instr: Instruction) (mach : MachineState) = failwith "Not Implemented"

    let processSimpleR (instr: Instruction) (mach : MachineState) =
        let localMap = Map [(AND,opAND);(OR, opOR); (SRAV,opSRAV); (XOR, opXOR);]
        let rs = getReg instr.rs mach
        let rt = getReg instr.rt mach
        let fn = Map.find instr.opcode localMap
        let output = fn mach instr rs rt
        setReg instr.rd output mach
    
    let processShiftR (instr : Instruction) (mach : MachineState) =
        let localMap = Map [(SRA, opSRA)]
        let rs = getReg instr.rs mach
        let rt = getReg instr.rt mach
        let fn = Map.find instr.opcode localMap
        let output = fn mach instr rs rt instr.shift
        setReg instr.rd output mach

    let opTypeMap = Map [([DIV; DIVU; MULT; MULTU],processMultDiv);
                        ([MFHI; MFLO; MTHI; MTLO],processHILO);
                        ([AND; XOR; OR],processSimpleR);
                        ([SRA],processShiftR)
                        ]

    let executeInstruction (instr : Instruction) (mach : MachineState) =
        let key = Map.findKey (fun x _ -> List.contains instr.opcode x) opTypeMap //Could be slightly simpler with a map.findWith style
        let fn = Map.find key opTypeMap
        fn instr mach
