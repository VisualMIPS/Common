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
        let rs = T.getValue(getReg instr.rs mach)
        let rt = T.getValue(getReg instr.rt mach)
        let tmp = 
             match instr.opcode with
             | ADDU -> rs + rt
             | AND -> rs &&& rt
             | OR -> rs ||| rt
             | SRAV -> uint32(int32 rt >>> int32 rs)
             | SRLV -> rt >>> int32 rs
             | SLLV -> rt <<< int32 rs
             | SUBU -> rs - rt
             | XOR -> rs ^^^ rt
             | SLT -> 
                match int32(rs) < int32(rt) with
                | true -> 1u 
                | false -> 0u
             | SLTU ->
                match rs < rt with
                | true -> 1u
                | false -> 0u
             | MFHI -> T.getValue(mach.Hi)
             | MFLO -> T.getValue(mach.Lo)
             | _ -> failwith "opcode does not belong to processSimpleR functions"
        let output = Word(tmp)
        setReg instr.rd output mach
    
    let processShiftR (instr : Instruction) (mach : MachineState) =
        let rt = T.getValue(getReg instr.rt mach)
        let shiftval = int32 (T.getValue(instr.shift))
        let tmp = 
            match instr.opcode with 
            | SRA -> uint32 (int32 rt >>> shiftval)
            | SRL -> rt >>> shiftval
            | SLL -> rt <<< shiftval
            | _ -> failwith "opcode does not belong to processShiftR functions"
        let output = Word(tmp)
        setReg instr.rd output mach
    
   (* let processFullR (instr: Instruction) (mach : MachineState) =
        let localMap = Map [(ADD, opADD)]
        let rs = getReg instr.rs mach
        let rt = getReg instr.rt mach
        let fn = Map.find instr.opcode localMap
        let output = fn mach instr rs rt 
    *)

    
    // --------------- //

    let opTypeMap = Map [([DIV; DIVU; MULT; MULTU],processMultDiv);
                        ([MFHI; MFLO; MTHI; MTLO],processHILO);
                        ([AND; XOR; OR],processSimpleR);
                        ([SRA],processShiftR)
                        ]

    let executeInstruction (instr : Instruction) (mach : MachineState) =
        let key = Map.findKey (fun x _ -> List.contains instr.opcode x) opTypeMap //Could be slightly simpler with a map.findWith style
        let fn = Map.find key opTypeMap
        fn instr mach
