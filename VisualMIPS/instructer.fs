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
    | ADDU ->
    | AND -> opAND mach instr rS rT //repetitive, can do better, with a curried function or smg
    | OR -> opOR mach instr rS rT
    | SRA -> opSRA mach instr rS rT
    | SRAV | SRL | SRLV | SLL | SLLV | SUB | SUBU 
    | XOR -> opXOR mach instr rS rT
    | SLT | SLTU | DIV | DIVU | MULT | MULTU 
    | JR | JALR | MFHI | MFLO | MTHI | MTLO

let opADD (mach: MachineState, R_type: instr, Word rS, Word rT) = // to be coded

let opAND (mach: MachineState, R_type: instr, Word rS, Word rT) =
    mach.setReg instr.r_d (rS &&& rT)

let opOR (mach: MachineState, R_type: instr, Word rS, Word rT) =
    mach.setReg instr.r_d (rS ||| rT)

let opSRA (mach: MachineState, R_type: instr, Word rS, Word rT) =
      mach.setReg instr.r_d (rS >>> instr.shift) //should pass shift already retrieved ?

let opXOR (mach: MachineState, R_type: instr, Word rS, Word rT) =
    mach.setReg instr.r_d (rS ^^^ rT)
    
