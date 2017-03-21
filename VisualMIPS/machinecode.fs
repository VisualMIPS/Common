namespace VisualMIPS

module MachineCode =
    
    open Types
    open Instructions

    type MachCode = MachCode of uint32

    let convertI_Type (instr: Instruction) =
        let opcode = Map.find instr.opcode ICodeMap <<< 26
        let rs = T.getValue instr.rs <<< 21
        let rt = T.getValue instr.rt <<< 16
        let imm = T.getValue instr.immed
        // Return Machine Code as uint32 rather than binary
        let code = uint32(opcode + rs + rt + int(uint16(imm)))
        code

    let convertI_OType (instr: Instruction) =
        let opcode = Map.find instr.opcode I_OCodeMap <<< 26
        let rs = T.getValue instr.rs <<< 21
        let rt = T.getValue instr.rt <<< 16
        let offset = T.getValue instr.immed
        // Return Machine Code as uint32 rather than binary
        let code = uint32(opcode + rs + rt + int(uint16(offset)))
        code

    let convertI_SType (instr: Instruction) =
        let opcode = 0b000001
        let rs = T.getValue instr.rs <<< 21
        let op = Map.find instr.opcode I_SCodeMap <<< 16
        let offset = T.getValue instr.immed
        // Return Machine Code as uint32 rather than binary
        let code = uint32(opcode + rs + op + int(uint16(offset)))
        code

    let convertI_SOType (instr: Instruction) =
        let opcode = Map.find instr.opcode I_SOCodeMap <<< 26
        let rs = T.getValue instr.rs <<< 21
        let rt = T.getValue instr.rt <<< 16
        let offset = T.getValue instr.immed
        // Return Machine Code as uint32 rather than binary
        let code = uint32(opcode + rs + rt + int(uint16(offset)))
        code

    let convertI_BOType (instr: Instruction) =
        let opcode = Map.find instr.opcode I_BOCodeMap <<< 26
        let rs = T.getValue instr.rs <<< 21
        let rt = T.getValue instr.rt <<< 16
        let offset = T.getValue instr.immed
        // Return Machine Code as uint32 rather than binary
        let code = uint32(opcode + rs + rt + int(uint16(offset)))
        code

    let convertJ_Type (instr: Instruction) =
        let opcode = Map.find instr.opcode JCodeMap <<< 26
        let target = T.getValue instr.target
        // Return Machine Code as uint32 rather than binary
        let code = uint32(opcode + int(uint32(target)))
        code

    let convertR_Type (instr: Instruction) =
        let opcode = 0  // All R Types have opcode 0
        let rs = T.getValue instr.rs <<< 21
        let rt = T.getValue instr.rt <<< 16
        let rd = T.getValue instr.rd <<< 11
        let func = Map.find instr.opcode RCodeMap
        // Return Machine Code as uint32 rather than binary
        let code = uint32(opcode + rs + rt + rd + func)
        code

    let convertR_SType (instr: Instruction) =
        let opcode = 0  // All R Types have opcode 0
        let rt = T.getValue instr.rt <<< 16
        let rd = T.getValue instr.rd <<< 11
        let shift = T.getValue instr.shift <<< 6
        let func = Map.find instr.opcode R_SCodeMap
        // Return Machine Code as uint32 rather than binary
        let code = uint32(opcode + rt + rd + int(shift)+ func)
        code

    let convertR_JType (instr: Instruction) =
        let opcode = 0  // All R Types have opcode 0
        let rs = T.getValue instr.rs <<< 21
        let rd = T.getValue instr.rd <<< 11
        let func = Map.find instr.opcode R_JCodeMap
        // Return Machine Code as uint32 rather than binary
        let code = uint32(opcode + rs + rd + func)
        code

    let convertR_MType (instr: Instruction) =
        let opcode = 0  // All R Types have opcode 0
        let rs = T.getValue instr.rs <<< 21
        let rt = T.getValue instr.rt <<< 16
        let func = Map.find instr.opcode R_MCodeMap
        // Return Machine Code as uint32 rather than binary
        let code = uint32(opcode + rs + rt + func)
        code
    
    /// Converts an Instruction into a Machine Code of type uint32, not binary.
    let convert (instr: Instruction) =
        match instr with
        | x when instr.instr_type = I -> convertI_Type x
        | x when instr.instr_type = I_O -> convertI_OType x
        | x when instr.instr_type = I_S -> convertI_SType x
        | x when instr.instr_type = I_SO -> convertI_SOType x
        | x when instr.instr_type = I_BO -> convertI_BOType x
        | x when instr.instr_type = JJ -> convertJ_Type x
        | x when instr.instr_type = R -> convertR_Type x
        | x when instr.instr_type = R_V -> convertR_Type x
        | x when instr.instr_type = R_J -> convertR_JType x
        | x when instr.instr_type = R_M -> convertR_MType x
        | _ -> failwith "Invalid Instruction!"