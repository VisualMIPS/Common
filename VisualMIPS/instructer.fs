//Decode/depatch
namespace VisualMIPS

module Executor =
    open Types
    open Instructions
    open MachineState
    open Rtypes

    let processMultDiv (instr: Instruction)  (mach : MachineState)= failwith "Not Implemented"

    let processHILO (instr: Instruction) (mach : MachineState) = failwith "Not Implemented"

    let processBranchI (instr: Instruction) (mach : MachineState) =
        let immed = (T.getValue instr.immed)
        let rs = T.getValue( getReg instr.rs mach )
        let rt = T.getValue( getReg instr.rt mach )//Only used in a couple.
        let (branch, link) = match instr.opcode with
                                | BGEZ when rs >= 0u -> (true,false)
                                | BGEZAL when rs >= 0u -> (true,true)
                                | BEQ when rs = rt -> (true,false)
                                | BNE when rs <> rt -> (true,false)
                                | BLEZ when rs <= 0u -> (true, false)
                                | BLTZ when rs < 0u -> (true, false)
                                | BLTZAL when rs < 0u -> (true, true)
                                | BGTZ when rs >= 0u -> (true, false)
                                //FIXME: Do the link commands always link? Spec seems to suggest that.
                                | _ -> (false, false)
        setNextNextPC (Word ((getNextPC mach |> T.getValue) + 4u*(uint32 immed))) //need to sign extend when converting to uint32

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
