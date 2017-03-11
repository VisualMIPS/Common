//Decode/depatch

executeInstr -> MachineState -> Instruction -> MachineState

let executeInstruction (machIn : MachineState, instr : Instruction) =
    match instr with:
    | I iInstr -> executeIType machIn iInstr
    | J jInstr -> executeJType machIn jInstr
    | R rInstr -> executeRType machIn rInstr
    
    
let executeJType (mach : MachineState, J_Type : instr) =
    match J_Type.opcode with
    | J -> 
    | JAL ->

let executeIType (mach : MachineState, I_type : instr) =

let executeRType (mach : MachineState, R_type : instr) =
    let rS = mach.getReg instr.r_s
    let rT = mach.getReg instr.r_t
    
    match R_Type.opcode with
    | ADD -> opADD mach instr rS rT



let opAND (mach: MachineState, R_type: instr, Word rS, Word rT) =
    mach.setReg instr.r_d (rS &&& rT)
    
