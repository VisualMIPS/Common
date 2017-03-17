//Decode/depatch
namespace VisualMIPS

module Executor =
    open Types
    open Instructions
    open MachineState
    open Rtypes
    open Itypes


    // R execution
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
             | MFHI -> T.getValue(getHi mach)
             | MFLO -> T.getValue(getLo mach)
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
        

    let processFullR (instr: Instruction) (mach : MachineState) =
        let localMap1 = Map[(ADD,opADD);(SUB,opSUB);(JALR,opJALR)] //can change rd
        let localMap2 = Map[(DIV,opDIV);(DIVU,opDIVU);(MULT,opMULT);(MULTU,opMULTU);
                        (JR,opJR);(MTHI,opMTHI);(MTLO,opMTLO)] //no need to change rd
        let rs = getReg instr.rs mach
        let rt = getReg instr.rt mach
        let returnmach =
            match (Map.containsKey instr.opcode localMap1) with
            | true ->
                let fn1 = Map.find instr.opcode localMap1
                let (output,newmach) = fn1 mach instr rs rt
                setReg instr.rd output newmach 
            | false ->
                let fn2 = Map.find instr.opcode localMap2
                let newmach = fn2 mach instr rs rt
                newmach 
        returnmach 
    
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

    // I execution

    let processSimpleI (instr: Instruction) (mach : MachineState) =
        let rs = T.getValue(getReg instr.rs mach)
        let rt = T.getValue(getReg instr.rt mach)
        let immediate = uint32(T.getValue instr.immed) //pads with 16-bit of MSB's value
        let tmp = 
             match instr.opcode with
             | ADDIU -> rs + immediate //pad with MSB ????
             | ANDI -> rs &&& immediate //pad with zeros
             | ORI -> rs ||| immediate
             | XORI -> rs ^^^ immediate
             | _ -> failwith "opcode does not belong to processSimpleI functions"
        let output = Word(tmp)
        setReg instr.rt output mach
    // --------------- //

    // Dispatch execution

    let opTypeMap = Map [
                        ([ADDU; AND; OR; SRAV; SRLV; SLLV; SUBU; XOR; SLT; SLTU; MFHI; MFLO], processSimpleR);
                        ([SRA; SRL; SLL], processShiftR);
                        ([ADD; SUB; JALR; DIV; DIVU; MULT; MULTU; JR; MTHI; MTLO], processFullR);
                        ([ADDIU; ANDI; ORI; XORI],processSimpleI);
                        ([DIV; DIVU; MULT; MULTU],processMultDiv);
                        ([MFHI; MFLO; MTHI; MTLO],processHILO);
                        ]

    let executeInstruction (instr : Instruction) (mach : MachineState) =
        let key = Map.findKey (fun x _ -> List.contains instr.opcode x) opTypeMap //Could be slightly simpler with a map.findWith style
        let fn = Map.find key opTypeMap
        fn instr mach
