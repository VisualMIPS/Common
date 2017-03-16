namespace VisualMIPS

module Parser =

    open Types
    open Instructions

    (*// Placeholder functions from Tick4, will be replaced.
    // Need Error responses for different parsing situations
    type parseResponse =
        | VarValue of int // for commands that return data
        | ParseError // if command string is invalid
        | OK // for valid commads that return no data
    
    open System.Text.RegularExpressions
    // Need alpha (for function names) and num (for values) to identify valid tokens
    let isAlpha s = Regex.IsMatch (s, @"^[a-zA-Z]+$")

    let isNum s = Regex.IsMatch (s, @"^-?[0-9]+$")*)

    // Need parse function to take in tokens and output instruction type

    /// Parse (Opcode rt, rs, immediate)
    let parseI_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] IMap
        let r_t = Register(int(iTokens.[1]))
        let r_s = Register(int(iTokens.[2]))
        let immed = Half(uint16(iTokens.[3]))
        {opcode=opcode; instr_type = I; rs=r_s; rt=r_t; rd=Register(0); shift=Shiftval(0uy); immed=immed; target=Targetval(0u)}

    /// Parse (Opcode rs, rt, offset)
    let parseI_O_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_OMap
        let r_s = Register(int(iTokens.[1]))
        let r_t = Register(int(iTokens.[2]))
        let immed = Half(uint16(iTokens.[3]))
        {opcode=opcode; instr_type = I_O; rs=r_s; rt=r_t; rd=Register(0); shift=Shiftval(0uy); immed=immed; target=Targetval(0u)}

    /// Parse (Opcode rs, offset) Same as I_SO
    let parseI_S_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_SMap
        let r_s = Register(int(iTokens.[1]))
        let immed = Half(uint16(iTokens.[2]))
        {opcode=opcode; instr_type = I_S; rs=r_s; rt=Register(0); rd=Register(0); shift=Shiftval(0uy); immed=immed; target=Targetval(0u)}
    
    /// Parse (Opcode rs, offset) Same as I_S
    let parseI_SO_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_SOMap
        let r_s = Register(int(iTokens.[1]))
        let immed = Half(uint16(iTokens.[2]))
        {opcode=opcode; instr_type = I_SO; rs=r_s; rt=Register(0); rd=Register(0); shift=Shiftval(0uy); immed=immed; target=Targetval(0u)}
    
    /// Parse (Opcode rt, offset(rs))
    let parseI_BO_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_BOMap
        let r_t = Register(int(iTokens.[1]))
        let immed = Half(uint16(iTokens.[2]))
        let r_s = Register(int(iTokens.[3]))
        {opcode=opcode; instr_type = I_BO; rs=r_s; rt=r_t; rd=Register(0); shift=Shiftval(0uy); immed=immed; target=Targetval(0u)}

    /// Parse (Opcode target)
    let parseJ_Type (jTokens: string[]) =            
        let opcode = Map.find jTokens.[0] JMap
        let target = Targetval(uint32(jTokens.[1]))
        {opcode=opcode; instr_type = J; rs=Register(0); rt=Register(0); rd=Register(0); shift=Shiftval(0uy); immed=Half(0us); target=target}

    /// Parse (Opcode rd, rs, rt)
    let parseR_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] RMap
        let r_d = Register(int(rTokens.[1]))
        let r_s = Register(int(rTokens.[2]))
        let r_t = Register(int(rTokens.[3]))
        {opcode=opcode; instr_type = R; rs=r_s; rt=r_t; rd=r_d; shift=Shiftval(0uy); immed=Half(0us); target=Targetval(0u)}
    
    /// Parse (Opcode rd, rt, rs)
    let parseR_V_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] R_VMap
        let r_d = Register(int(rTokens.[1]))
        let r_t = Register(int(rTokens.[2]))
        let r_s = Register(int(rTokens.[3]))
        {opcode=opcode; instr_type = R_V; rs=r_s; rt=r_t; rd=r_d; shift=Shiftval(0uy); immed=Half(0us); target=Targetval(0u)}

    /// Parse (Opcode rd, rt, shift)
    let parseR_S_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] R_SMap
        let r_d = Register(int(rTokens.[1]))
        let r_t = Register(int(rTokens.[2]))
        let shift = Shiftval(byte(rTokens.[3]))
        {opcode=opcode; instr_type = R_S; rs=Register(0); rt=r_t; rd=r_d; shift=shift; immed=Half(0us); target=Targetval(0u)}

    /// Parse (Opcode rd, rs) or (Opcode rs)
    let parseR_J_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] R_JMap
        if opcode = JALR then
            if rTokens.Length = 3 then
                let r_d = Register(int(rTokens.[1]))
                let r_s = Register(int(rTokens.[2]))
                {opcode=opcode; instr_type = R_J; rs=r_s; rt=Register(0); rd=r_d; shift=Shiftval(0uy); immed=Half(0us); target=Targetval(0u)}
            else
                let r_d = Register(31)
                let r_s = Register(int(rTokens.[1]))
                {opcode=opcode; instr_type = R_J; rs=r_s; rt=Register(0); rd=r_d; shift=Shiftval(0uy); immed=Half(0us); target=Targetval(0u)}
        else
            let r_s = Register(int(rTokens.[1]))
            {opcode=opcode; instr_type = R_J; rs=r_s; rt=Register(0); rd=Register(0); shift=Shiftval(0uy); immed=Half(0us); target=Targetval(0u)}
    
    /// Parses Token into Instruction
    let parse (tokens: string[]) =
        if Map.containsKey tokens.[0] IMap then parseI_Type tokens
        elif Map.containsKey tokens.[0] I_OMap then parseI_O_Type tokens
        elif Map.containsKey tokens.[0] I_SMap then parseI_S_Type tokens
        elif Map.containsKey tokens.[0] I_SOMap then parseI_SO_Type tokens
        elif Map.containsKey tokens.[0] I_BOMap then parseI_BO_Type tokens
        elif Map.containsKey tokens.[0] JMap then parseJ_Type tokens
        elif Map.containsKey tokens.[0] RMap then parseR_Type tokens
        elif Map.containsKey tokens.[0] R_VMap then parseR_V_Type tokens
        elif Map.containsKey tokens.[0] R_SMap then parseR_S_Type tokens
        elif Map.containsKey tokens.[0] R_JMap then parseR_J_Type tokens
        else failwith "Invalid Opcode: Does not exist in MIPS I!"

    // Print instruction helper functions
    let printI_Type (instr: Instruction) =
        printfn "Opcode: %A, rs: %A, rt: %A, imm: %A" instr.opcode instr.rs instr.rt instr.immed

    let printI_O_Type (instr: Instruction) =
        printfn "Opcode: %A, rs: %A, rt: %A, offset: %A" instr.opcode instr.rs instr.rt instr.immed

    let printI_SO_Type (instr: Instruction) =
        printfn "Opcode: %A, rs: %A, offset: %A" instr.opcode instr.rs instr.immed

    let printI_BO_Type (instr: Instruction) =
        printfn "Opcode: %A, rt: %A, offset: %A, rs: %A" instr.opcode instr.rt instr.immed instr.rs

    let printJ_Type (instr: Instruction) =
        printfn "Opcode: %A, target: %A" instr.opcode instr.target

    let printR_Type (instr: Instruction) =
        printfn "Opcode: %A, rs: %A, rt: %A, rd: %A" instr.opcode instr.rs instr.rt instr.rd

    let printR_V_Type (instr: Instruction) =
        printfn "Opcode: %A, rt: %A, rs: %A, rd: %A" instr.opcode instr.rt instr.rs instr.rd

    let printR_S_Type (instr: Instruction) =
        printfn "Opcode: %A, rt: %A, rd: %A, shift: %A" instr.opcode instr.rt instr.rd instr.shift

    let printR_J_Type (instr: Instruction) =
        if instr.opcode = JALR then printfn "Opcode: %A, rd: %A, rs: %A" instr.opcode instr.rd instr.rs
        else printfn "Opcode: %A, rs: %A" instr.opcode instr.rs
    
    /// Prints parsed Instruction for debugging
    let printInstr (instr: Instruction) =
        match instr with
        | x when instr.instr_type = I -> printI_Type x
        | x when instr.instr_type = I_O -> printI_O_Type x
        | x when instr.instr_type = I_S -> printI_SO_Type x
        | x when instr.instr_type = I_SO -> printI_SO_Type x
        | x when instr.instr_type = I_BO -> printI_BO_Type x
        | x when instr.instr_type = J -> printJ_Type x
        | x when instr.instr_type = R -> printR_Type x
        | x when instr.instr_type = R_V -> printR_V_Type x
        | x when instr.instr_type = R_S -> printR_S_Type x
        | x when instr.instr_type = R_J -> printR_J_Type x
        | _ -> failwith "Invalid Instruction!"