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
                let newmach1 = setReg instr.rd output newmach 
                newmach1
            | false ->
                let fn2 = Map.find instr.opcode localMap2
                let newmach2 = fn2 mach instr rs rt
                newmach2 
        returnmach 
    
    let processMultDiv (instr: Instruction)  (mach : MachineState)= failwith "Not Implemented"

    let processHILO (instr: Instruction) (mach : MachineState) = failwith "Not Implemented"

    // I execution

    let processSimpleI (instr: Instruction) (mach : MachineState) =
        let rs = T.getValue(getReg instr.rs mach)
        let rt = T.getValue(getReg instr.rt mach)
        let immediateForAdd = uint32(T.getValue instr.immed) //pads with 16-bit of MSB's value
        let immediateForRest = immediateForAdd &&& 65535u //pads with zeros regardless of MSB
        //65535 is 00000000000000001111111111111111 in binary
        let tmp = 
             match instr.opcode with
             | ADDIU -> rs + immediateForAdd
             | ANDI -> rs &&& immediateForRest 
             | ORI -> rs ||| immediateForRest
             | XORI -> rs ^^^ immediateForRest
             | LUI -> immediateForAdd <<< 16
             | SLTI ->
                match int32(rs) < int32(immediateForAdd) with
                | true -> 1u 
                | false -> 0u
             | SLTIU ->
                match rs < immediateForAdd with
                | true -> 1u
                | false -> 0u
             | _ -> failwith "opcode does not belong to processSimpleI functions"
        let output = Word(tmp)
        let newmach = setReg instr.rt output mach
        newmach

    
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
        let newmach = setNextNextPC (Word ((getNextPC mach |> T.getValue) + 4u*(uint32 immed))) mach//need to sign extend when converting to uint32
        newmach

    let processfullI (instr: Instruction) (mach : MachineState) =
        let localMap = Map[(ADDI,opADDI)]
        let rs = getReg instr.rs mach
        let rt = getReg instr.rt mach
        let immediate = int32(T.getValue instr.immed)
        let fn = Map.find instr.opcode localMap
        let (output,newmach) = fn mach instr rs rt
        let newmach = setReg instr.rt output newmach
        newmach

    // --------------- //

    // Dispatch execution

    let opTypeMap = Map [
                        ([ADDU; AND; OR; SRAV; SRLV; SLLV; SUBU; XOR; SLT; SLTU; MFHI; MFLO], processSimpleR);
                        ([SRA; SRL; SLL], processShiftR);
                        ([ADD; SUB; JALR; DIV; DIVU; MULT; MULTU; JR; MTHI; MTLO], processFullR);
                        ([ADDIU; ANDI; ORI; XORI; LUI; SLTI; SLTIU],processSimpleI);
                        ([BGEZ; BGEZAL; BEQ; BNE; BLEZ; BLTZ; BLTZAL; BGTZ], processBranchI);
                        ([ADDI], processfullI);
                        ([DIV; DIVU; MULT; MULTU],processMultDiv);
                        ([MFHI; MFLO; MTHI; MTLO],processHILO);
                        ]

    let executeInstruction (instr : Instruction) (mach : MachineState) =
        let key = Map.findKey (fun x _ -> List.contains instr.opcode x) opTypeMap //Could be slightly simpler with a map.findWith style
        let fn = Map.find key opTypeMap
        fn instr mach
