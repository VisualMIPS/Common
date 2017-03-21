namespace VisualMIPS

/// ===========================================
/// Global types for this project
/// ===========================================

module Types =

    type Word = Word of uint32
    type Half = Half of uint16
    type Byte = Byte of byte

    type Register = Register of int
    type Memory = Memory of uint32
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
                    | BNE | LB | LBU | LH | LHU| LW | LWL | LWR | SB | SH 
                    | SW | LUI | SLTI | SLTIU | J | JAL | ADD | ADDU | AND | OR | SRA 
                    | SRAV | SRL | SRLV | SLL | SLLV | SUB | SUBU 
                    | XOR | SLT | SLTU | DIV | DIVU | MULT | MULTU 
                    | JR | JALR | MFHI | MFLO | MTHI | MTLO | NOR

    let IMap = Map [("ADDI", ADDI);
                    ("ADDIU", ADDIU);
                    ("ANDI", ANDI);
                    ("ORI", ORI);
                    ("XORI", XORI);
                    ("SLTI", SLTI);
                    ("SLTIU", SLTIU)]

    let I_OMap = Map [("BEQ", BEQ);
                      ("BNE", BNE)]

    let I_SMap = Map [("BGEZ", BGEZ);
                      ("BGEZAL", BGEZAL);
                      ("BLTZAL", BLTZAL)]
                       
    let I_SOMap = Map [("BGTZ", BGTZ);
                       ("BLEZ", BLEZ);
                       ("BLTZ", BLTZ);
                       ("LUI", LUI)]

    let I_BOMap = Map [("LB", LB);
                       ("LBU", LBU);
                       ("LH", LH);
                       ("LHU", LHU);
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
                    ("NOR", NOR)
                    ("SUB", SUB);
                    ("SUBU", SUBU);
                    ("XOR", XOR);
                    ("SLT", SLT);
                    ("SLTU", SLTU)]

    let R_SMap = Map [("SRA", SRA);
                      ("SRL", SRL);
                      ("SLL", SLL)]

    let R_VMap = Map [("SRAV", SRAV);
                      ("SRLV", SRLV);
                      ("SLLV", SLLV)]

    let R_JMap = Map [("JR", JR);
                      ("JALR", JALR);
                      ("MFHI", MFHI);
                      ("MFLO", MFLO);
                      ("MTHI", MTHI);
                      ("MTLO", MTLO)]

    let R_MMap = Map [("DIV", DIV);
                      ("DIVU", DIVU);
                      ("MULT", MULT);
                      ("MULTU", MULTU)]

    // Maps with Instruction Opcode as key and MachineCode Opcode as value

    let ICodeMap = Map [(ADDI,  0b001000);
                        (ADDIU, 0b001001);
                        (ANDI,  0b001100);
                        (ORI,   0b001101);
                        (XORI,  0b001110);
                        (SLTI,  0b001010);
                        (SLTIU, 0b001011)]

    let I_OCodeMap = Map [(BEQ, 0b000100);
                          (BNE, 0b000101)]

    let I_SOCodeMap = Map [(BGEZ,   0b00001);
                           (BGEZAL, 0b10001);
                           (BLTZAL, 0b10000)]

    let I_SCodeMap = Map  [(BGTZ,   0b000111);
                           (BLEZ,   0b000110);
                           (BLTZ,   0b000001);                           
                           (LUI,    0b001111)]

    let I_BOCodeMap = Map [(LB,     0b100000);
                           (LBU,    0b100100);
                           (LH,     0b100001);
                           (LHU,    0b100101);
                           (LW,     0b100011);
                           (LWL,    0b100010);
                           (LWR,    0b100110);
                           (SB,     0b101000);
                           (SH,     0b101001);
                           (SW,     0b101011)]

    let JCodeMap = Map [(J,   0b10);
                        (JAL, 0b11)]

    let RCodeMap = Map [(ADD,   0b100000);
                        (ADDU,  0b100001);
                        (AND,   0b100100);
                        (OR,    0b100101);
                        (NOR,   0b100111)
                        (SUB,   0b100010);
                        (SUBU,  0b100011);
                        (XOR,   0b100110);
                        (SLT,   0b101010);
                        (SLTU,  0b101011)]
                        
    let R_SCodeMap = Map [(SRA,     0b000011);
                          (SRL,     0b000010);
                          (SLL,     0b000000)]

    let R_VCodeMap = Map [(SRAV,    0b000111);
                          (SRLV,    0b000110);
                          (SLLV,    0b000100)]                    
   
                        
    let R_JCodeMap = Map [(JR,      0b001000);
                          (JALR,    0b001001);
                          (MFHI,    0b010000);
                          (MFLO,    0b010010);              
                          (MTHI,    0b010001);
                          (MTLO,    0b010011)]

    let R_MCodeMap = Map [(DIV,   0b011010);
                          (DIVU,  0b011011);
                          (MULT,  0b011000);
                          (MULTU, 0b011001)]

    type Instr_Type = | I | JJ | R | I_O | I_S | I_SO | I_BO | R_V | R_S | R_J | R_M

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