namespace VisualMIPS

/// ===========================================
/// Global types for this project
/// ===========================================

module Types =

    type Word = Word of uint32
    type Half = Half of uint16
    type Byte = Byte of byte

    type Register = Register of int
    type Memory = Memory of int
    type Shiftval = Shiftval of byte
    type Targetval = Targetval of uint32

    type T =
        static member getValue (Shiftval v) = v
        static member getValue (Targetval v) = v
        static member getValue (Word v) = v
        static member getValue (Half v) = v
        static member getValue (Byte v) = v
        static member getValue (Register v) = v
        static member getValue (Memory v) = v



/// ===========================================
/// Instruction types
/// ===========================================
module Instructions =
    
    open Types

    type Opcode = | ADDI | ADDIU | ANDI | ORI | XORI 
                    | BEQ | BGEZ | BGEZAL | BGTZ | BLEZ | BLTZ | BLTZAL 
                    | BNE | LB | LBU | LH | LW | LWL | LWR | SB | SH 
                    | SW | LUI | SLTI | SLTIU | J | JAL | ADD | ADDU | AND | OR | SRA 
                    | SRAV | SRL | SRLV | SLL | SLLV | SUB | SUBU 
                    | XOR | SLT | SLTU | DIV | DIVU | MULT | MULTU 
                    | JR | JALR | MFHI | MFLO | MTHI | MTLO

    let IMap = Map [("ADDI", ADDI);
                    ("ADDIU", ADDIU);
                    ("ANDI", ANDI);
                    ("ORI", ORI);
                    ("XORI", XORI);
                    ("SLTI", SLTI);
                    ("SLTIU", SLTIU)]

    let I_OMap = Map [("BEQ", BEQ);
                      ("BNE", BNE)]

    let I_SOMap = Map [("BGEZ", BGEZ);
                       ("BGEZAL", BGEZAL);
                       ("BGTZ", BGTZ);
                       ("BLEZ", BLEZ);
                       ("BLTZ", BLTZ);
                       ("BLTZAL", BLTZAL);
                       ("LUI", LUI)]

    let I_BOMap = Map [("LB", LB);
                       ("LBU", LBU);
                       ("LH", LH);
                       ("LW", LW);
                       ("LWL", LWL);
                       ("LWR", LWR);
                       ("SB", SB);
                       ("SH", SH);
                       ("SW", SW)]

    let JMap = Map [("J", J);
                    ("JAL", JAL)]
    
    let RMap = Map [("ADD", ADD);
                    ("ADDU", ADDU);
                    ("AND", AND);
                    ("OR", OR);
                    ("SUB", SUB);
                    ("SUBU", SUBU);
                    ("XOR", XOR);
                    ("SLT", SLT);
                    ("SLTU", SLTU);
                    ("DIV", DIV);
                    ("DIVU", DIVU);
                    ("MULT", MULT);
                    ("MULTU", MULTU);
                    ("MFHI", MFHI);
                    ("MFLO", MFLO);
                    ("MTHI", MTHI);
                    ("MTLO", MTLO)]

    let R_SMap = Map [("SRA", SRA);
                      ("SRL", SRL);
                      ("SLL", SLL)]

    let R_VMap = Map [("SRAV", SRAV);
                      ("SRLV", SRLV);
                      ("SLLV", SLLV)]

    let R_JMap = Map [("JR", JR);
                      ("JALR", JALR);]

    type Instr_Type = | I | J | R | I_O | I_SO | I_BO | R_V | R_S | R_J

    type Instruction =
        {
        opcode : Opcode
        instr_type : Instr_Type
        rs : Register
        rt : Register
        rd : Register
        shift : Shiftval
        immed : Half
        target : Targetval
        }

