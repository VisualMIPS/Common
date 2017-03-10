namespace VisualMIPS

/// ===========================================
/// Global types for this project
/// ===========================================
module Types =

    type Word = Word of uint32
    type Half = Half of uint16
    type Byte = Byte of byte

    type Register = R of int
    type Memory = M of int



/// ===========================================
/// Instruction types
/// ===========================================
module Instructions =
    
    open Types

    type I_Opcode = | ADDI | ADDIU | ANDI | ORI | XORI 
                    | BEQ | BGEZ | BGEZAL | BGTZ | BLEZ | BLTZ | BLTZAL 
                    | BNE | LB | LBU | LH | LW | LWL | LWR | SB | SH 
                    | SW | LUI | SLTI | SLTIU 

    type J_Opcode = | J | JAL

    type R_Opcode = | ADD | ADDU | AND | OR | SRA 
                    | SRAV | SRL | SRLV | SLL | SLLV | SUB | SUBU 
                    | XOR | SLT | SLTU | DIV | DIVU | MULT | MULTU 
                    | JR | JALR | MFHI | MFLO | MTHI | MTLO

    type Shiftval = Shiftval of byte
    type Targetval = Targetval of uint32 

    type I_type =   
        {
        opcode : I_Opcode
        r_s : Register
        r_t : Register
        immed : Half
        }

    type J_type =
        {
        opcode : J_Opcode
        target : Targetval
        }

    type R_type =
        {
        opcode : R_Opcode
        r_s : Register
        r_t : Register
        r_d : Register
        shift : Shiftval
        }
    
    type Instruction =
        | I of I_type
        | J of J_type
        | R of R_type