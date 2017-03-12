//Decode/depatch
namespace VisualMIPS

module Executor =
    open Types
    open Instructions
    open MachineState
    open Stuff

    let processMultDiv (mach : MachineState) (instr: Instruction) = failwith "Not Implemented"

    let processHILO (mach : MachineState) (instr: Instruction) = failwith "Not Implemented"

    let processR (machIn : MachineState) (instr: Instruction) =
        let localMap = Map [(AND,opAND)] //;(ADDU,opADDU)]
        Map.find instr.opcode localMap
    (*
    let opTypeMap = Map    [([DIV; DIVU; MULT; MULTU],processMultDiv);
                        ([MFHI; MFLO; MTHI; MTLO],processHILO)
        ]

    ///Sweet code
    
    let executeRType (mach : MachineState) (instr : R_type) =
        let rS = getReg mach instr.r_s
        let rT = getReg mach instr.r_t
        match instr.opcode with
        | ADD -> failwith "Not Implemented"
        | ADDU -> failwith "Not Implemented"
        | AND -> opAND mach instr rS rT //repetitive, can do better, with a curried function or smg
        | OR -> opOR mach instr rS rT
        | SRA -> opSRA mach instr rS rT
        | SRAV | SRL | SRLV | SLL | SLLV | SUB | SUBU -> failwith "Not Implemented"
        | XOR -> opXOR mach instr rS rT
        | SLT | SLTU | DIV | DIVU | MULT | MULTU -> failwith "Not Implemented"
        | JR | JALR | MFHI | MFLO | MTHI | MTLO -> failwith "Not Implemented"
    

    let executeInstruction (machIn : MachineState, instr : Instruction) =
        let key = Map.findKey (fun x _ -> List.contains instr.opcode x) opTypeMap //Could be slightly simpler by addition
        let fn = Map.find key opTypeMap
        fn machIn instr
    *)

    //let opADD (mach: MachineState, R_type: instr, Word rS, Word rT) =  to be coded


