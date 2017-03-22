namespace VisualMIPS

module Parser =

    open Types
    open Instructions

    open System.Text.RegularExpressions
    // Need num (for values) to identify valid tokens

    let isNum s = Regex.IsMatch (s, @"^-?[0-9]+$")

    let regWithinRange (reg: int) = match reg with
                                    | x when x <= 31 && x >= 0 -> true
                                    | _ -> false

    let immWithinRange (imm: int) = match imm with
                                    | x when x <= 32767 && x >= -32768 -> true
                                    | _ -> false

    let targetWithinRange (target: int) =   match target with
                                            | x when x <= 67108863 && x >= 0 -> true
                                            | _ -> false
    
    // Need parse function to take in tokens and output instruction type

    /// Parse (Opcode rt, rs, immediate)
    let parseI_Type (iTokens: string[]) =               
        let opcode = Map.find iTokens.[0] IMap

        let r_t, r_s, immed =
            match iTokens with
            | i when i.Length <> 4 -> failwithf "Invalid Operation: %A. Takes 3 parameters." i.[0]
            | i when not (isNum i.[1]) -> failwithf "rt: %A is invalid. Please use integers only." i.[1]
            | i when not (regWithinRange (int i.[1])) -> failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." i.[1]
            | i when not (isNum i.[2]) -> failwithf "rs: %A is invalid. Please use integers only." i.[2]
            | i when not (regWithinRange (int i.[2])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." i.[2]
            | i when not (isNum i.[3]) -> failwithf "imm: %A is invalid. Please use integers only." i.[3]
            | i when not (immWithinRange (int i.[3])) -> failwithf "imm: %A is not within range. Accepted values between -32768 and 32767." i.[3]
            | i -> (i.[1] |> int |> Register), (i.[2] |> int |> Register), (i.[3] |> uint16 |> Half)

        {opcode=opcode; instr_type = I; rs=r_s; rt=r_t; rd=Register 0; shift=Shiftval 0uy; immed=immed; target=Targetval 0u}

    /// Parse (Opcode rs, rt, offset)
    let parseI_O_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_OMap

        let r_s, r_t, immed =
            match iTokens with
            | i when i.Length <> 4 -> failwithf "Invalid Operation: %A. Takes 3 parameters." i.[0]
            | i when not (isNum i.[1]) -> failwithf "rs: %A is invalid. Please use integers only." i.[1]
            | i when not (regWithinRange (int i.[1])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." i.[1]
            | i when not (isNum i.[2]) -> failwithf "rt: %A is invalid. Please use integers only." i.[2]
            | i when not (regWithinRange (int i.[2])) -> failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." i.[2]
            | i when not (isNum i.[3]) -> failwithf "imm: %A is invalid. Please use integers only." i.[3]
            | i when not (immWithinRange (int i.[3])) -> failwithf "imm: %A is not within range. Accepted values between -32768 and 32767." i.[3]
            | i -> (i.[1] |> int |> Register), (i.[2] |> int |> Register), (i.[3] |> uint16 |> Half)

        {opcode=opcode; instr_type = I_O; rs=r_s; rt=r_t; rd=Register 0; shift=Shiftval 0uy; immed=immed; target=Targetval 0u}

    /// Parse (Opcode rs, offset) Same as I_SO
    let parseI_S_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_SMap

        let r_s, immed =
            match iTokens with
            | i when i.Length <> 3 -> failwithf "Invalid Operation: %A. Takes 2 parameters." i.[0]
            | i when not (isNum i.[1]) -> failwithf "rs: %A is invalid. Please use integers only." i.[1]
            | i when not (regWithinRange (int i.[1])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." i.[1]
            | i when not (isNum i.[2]) -> failwithf "imm: %A is invalid. Please use integers only." i.[2]
            | i when not (immWithinRange (int i.[2])) -> failwithf "imm: %A is not within range. Accepted values between -32768 and 32767." i.[2]
            | i -> (i.[1] |> int |> Register), (i.[2] |> uint16 |> Half)

        {opcode=opcode; instr_type = I_S; rs=r_s; rt=Register 0; rd=Register 0; shift=Shiftval 0uy; immed=immed; target=Targetval 0u}
    
    /// Parse (Opcode rs, offset) Same as I_S
    let parseI_SO_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_SOMap

        let r_t, r_s, immed =
            match iTokens with
            | i when i.Length <> 3 -> failwithf "Invalid Operation: %A. Takes 2 parameters." i.[0]
            | i when not (isNum i.[1]) -> failwithf "rs: %A is invalid. Please use integers only." i.[1]
            | i when not (regWithinRange (int i.[1])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." i.[1]
            | i when not (isNum i.[2]) -> failwithf "imm: %A is invalid. Please use integers only." i.[2]
            | i when not (immWithinRange (int i.[2])) -> failwithf "imm: %A is not within range. Accepted values between -32768 and 32767." i.[2]
            | i when i.[0] = "LUI" -> (i.[1] |> int |> Register), Register 0, (i.[2] |> uint16 |> Half)
            | i -> Register 0, (i.[1] |> int |> Register), (i.[2] |> uint16 |> Half)
        
        {opcode=opcode; instr_type = I_SO; rs=r_s; rt=r_t; rd=Register 0; shift=Shiftval 0uy; immed=immed; target=Targetval 0u}
    
    /// Parse (Opcode rt, offset(rs))
    let parseI_BO_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_BOMap

        let r_t, immed, r_s =
            match iTokens with
            | i when i.Length <> 4 -> failwithf "Invalid Operation: %A. Takes 3 parameters." i.[0]
            | i when not (isNum i.[1]) -> failwithf "rt: %A is invalid. Please use integers only." i.[1]
            | i when not (regWithinRange (int i.[1])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." i.[1]
            | i when not (isNum i.[2]) -> failwithf "imm: %A is invalid. Please use integers only." i.[2]
            | i when not (immWithinRange (int i.[2])) -> failwithf "imm: %A is not within range. Accepted values between -32768 and 32767." i.[2]
            | i when not (isNum i.[3]) -> failwithf "rs: %A is invalid. Please use integers only." i.[3]
            | i when not (regWithinRange (int i.[3])) -> failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." i.[3]
            | i -> (i.[1] |> int |> Register), (i.[2] |> uint16 |> Half), (i.[3] |> int |> Register)

        {opcode=opcode; instr_type = I_BO; rs=r_s; rt=r_t; rd=Register 0; shift=Shiftval 0uy; immed=immed; target=Targetval 0u}

    /// Parse (Opcode target)
    let parseJ_Type (jTokens: string[]) =            
        let opcode = Map.find jTokens.[0] JMap

        let target =
            match jTokens with
            | j when j.Length <> 2 -> failwithf "Invalid Operation: %A. Takes 1 parameter." j.[0]
            | j when not (isNum j.[1]) -> failwithf "target: %A is invalid. Please use integers only." j.[1]
            | j when not (targetWithinRange (int j.[1])) -> failwithf "target: %A is not within range. Accepted values between 0 and 67108863." j.[1]
            | j -> j.[1] |> uint32 |> Targetval

        {opcode=opcode; instr_type = JJ; rs=Register 0; rt=Register 0; rd=Register 0; shift=Shiftval 0uy; immed=Half 0us; target=target}

    /// Parse (Opcode rd, rs, rt)
    let parseR_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] RMap

        let r_d, r_s, r_t =
            match rTokens with
            | r when r.Length <> 4 -> failwithf "Invalid Operation: %A. Takes 3 parameters." r.[0]
            | r when not (isNum r.[1]) -> failwithf "rd: %A is invalid. Please use integers only." r.[1]
            | r when not (regWithinRange (int r.[1])) -> failwithf "rd: %A is not within range. Accepted Registers between 0 and 31." r.[1]
            | r when not (isNum r.[2]) -> failwithf "rs: %A is invalid. Please use integers only." r.[2]
            | r when not (regWithinRange (int r.[2])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." r.[2]
            | r when not (isNum r.[3]) -> failwithf "rt: %A is invalid. Please use integers only." r.[3]
            | r when not (regWithinRange (int r.[3])) -> failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." r.[3]
            | r -> (r.[1] |> int |> Register), (r.[2] |> int |> Register), (r.[3] |> int |> Register)

        {opcode=opcode; instr_type = R; rs=r_s; rt=r_t; rd=r_d; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}
    
    /// Parse (Opcode rd, rt, rs)
    let parseR_V_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] R_VMap

        let r_d, r_t, r_s =
            match rTokens with
            | r when r.Length <> 4 -> failwithf "Invalid Operation: %A. Takes 3 parameters." r.[0]
            | r when not (isNum r.[1]) -> failwithf "rd: %A is invalid. Please use integers only." r.[1]
            | r when not (regWithinRange (int r.[1])) -> failwithf "rd: %A is not within range. Accepted Registers between 0 and 31." r.[1]
            | r when not (isNum r.[2]) -> failwithf "rt: %A is invalid. Please use integers only." r.[2]
            | r when not (regWithinRange (int r.[2])) -> failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." r.[2]
            | r when not (isNum r.[3]) -> failwithf "rs: %A is invalid. Please use integers only." r.[3]
            | r when not (regWithinRange (int r.[3])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." r.[3]
            | r -> (r.[1] |> int |> Register), (r.[2] |> int |> Register), (r.[3] |> int |> Register)
        
        {opcode=opcode; instr_type = R_V; rs=r_s; rt=r_t; rd=r_d; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}

    /// Parse (Opcode rd, rt, shift)
    let parseR_S_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] R_SMap

        let r_d, r_t, shift =
            match rTokens with
            | r when r.Length <> 4 -> failwithf "Invalid Operation: %A. Takes 3 parameters." r.[0]
            | r when not (isNum r.[1]) -> failwithf "rs: %A is invalid. Please use integers only." r.[1]
            | r when not (regWithinRange (int r.[1])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." r.[1]
            | r when not (isNum r.[2]) -> failwithf "rt: %A is invalid. Please use integers only." r.[2]
            | r when not (regWithinRange (int r.[2])) -> failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." r.[2]
            | r when not (isNum r.[3]) -> failwithf "shift: %A is invalid. Please use integers only." r.[3]
            | r when not (regWithinRange (int r.[3])) -> failwithf "shift: %A is not within range. Accepted shift values between 0 and 31." r.[3]
            | r -> (r.[1] |> int |> Register), (r.[2] |> int |> Register), (r.[3] |> byte |> Shiftval)

        {opcode=opcode; instr_type = R_S; rs=Register 0; rt=r_t; rd=r_d; shift=shift; immed=Half 0us; target=Targetval 0u}

    /// Parse (Opcode rd, rs) or (Opcode rs) or (Opcode rd)
    let parseR_J_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] R_JMap

        if opcode = JALR then
            if rTokens.Length = 3 then
                let r_d, r_s =
                    match rTokens with
                    | r when not (isNum r.[1]) -> failwithf "rd: %A is invalid. Please use integers only." r.[1]
                    | r when not (regWithinRange (int r.[1])) -> failwithf "rd: %A is not within range. Accepted Registers between 0 and 31." r.[1]
                    | r when not (isNum r.[2]) -> failwithf "rs: %A is invalid. Please use integers only." r.[2]
                    | r when not (regWithinRange (int r.[2])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." r.[2]
                    | r -> (r.[1] |> int |> Register), (r.[2] |> int |> Register)

                {opcode=opcode; instr_type = R_J; rs=r_s; rt=Register 0; rd=r_d; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}
            
            elif rTokens.Length = 2 then
                let r_s =
                    match rTokens with
                    | r when not (isNum r.[1]) -> failwithf "rs: %A is invalid. Please use integers only." r.[1]
                    | r when not (regWithinRange (int r.[1])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." r.[1]
                    | r -> (r.[1] |> int |> Register)

                {opcode=opcode; instr_type = R_J; rs=r_s; rt=Register 0; rd=Register 31; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}
            
            else failwithf "Invalid Operation: %A. Takes 1 or 2 parameters." rTokens.[0]

        elif rTokens.Length = 2 then
            if opcode = MFLO || opcode = MFHI then
                let r_d =
                    match rTokens with
                    | r when not (isNum r.[1]) -> failwithf "rd: %A is invalid. Please use integers only." r.[1]
                    | r when not (regWithinRange (int r.[1])) -> failwithf "rd: %A is not within range. Accepted Registers between 0 and 31." r.[1]
                    | r -> (r.[1] |> int |> Register)

                {opcode=opcode; instr_type = R_J; rs=Register 0; rt=Register 0; rd=r_d; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}
        
            else
                let r_s =
                    match rTokens with
                    | r when not (isNum r.[1]) -> failwithf "rs: %A is invalid. Please use integers only." r.[1]
                    | r when not (regWithinRange (int r.[1])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." r.[1]
                    | r -> (r.[1] |> int |> Register)
               
                {opcode=opcode; instr_type = R_J; rs=r_s; rt=Register 0; rd=Register 0; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}
        
        else failwithf "Invalid Operation: %A. Takes 1 parameter." rTokens.[0]

    /// Parse (Opcode rs, rt)
    let parseR_M_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] R_MMap

        let r_s, r_t =
            match rTokens with
            | r when r.Length <> 3 -> failwithf "Invalid Operation: %A. Takes 2 parameters." r.[0]
            | r when not (isNum r.[1]) -> failwithf "rs: %A is invalid. Please use integers only." r.[1]
            | r when not (regWithinRange (int r.[1])) -> failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." r.[1]
            | r when not (isNum r.[2]) -> failwithf "rt: %A is invalid. Please use integers only." r.[2]
            | r when not (regWithinRange (int r.[2])) -> failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." r.[2]
            | r -> (r.[1] |> int |> Register), (r.[2] |> int |> Register)

        {opcode=opcode; instr_type = R_M; rs=r_s; rt=r_t; rd=Register 0; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}

    /// Parses Token into Instruction
    let parse (tokens: string[]) =
        match tokens with
        | t when Map.containsKey t.[0] IMap -> parseI_Type t
        | t when Map.containsKey t.[0] I_OMap -> parseI_O_Type t
        | t when Map.containsKey t.[0] I_SMap -> parseI_S_Type t
        | t when Map.containsKey t.[0] I_SOMap -> parseI_SO_Type t
        | t when Map.containsKey t.[0] I_BOMap -> parseI_BO_Type t
        | t when Map.containsKey t.[0] JMap -> parseJ_Type t
        | t when Map.containsKey t.[0] RMap -> parseR_Type t
        | t when Map.containsKey t.[0] R_VMap -> parseR_V_Type t
        | t when Map.containsKey t.[0] R_SMap -> parseR_S_Type t
        | t when Map.containsKey t.[0] R_JMap -> parseR_J_Type t
        | t when Map.containsKey t.[0] R_MMap -> parseR_M_Type t
        | _ -> failwithf "Syntax Error! \nInvalid Operation: %A does not exist in MIPS I! \nIs your operation name all UPPERCASE?" tokens.[0]

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

    let printR_M_Type (instr: Instruction) = 
        printfn "Opcode: %A, rs: %A, rt: %A" instr.opcode instr.rs instr.rt
    
    /// Prints parsed Instruction for debugging
    let printInstr (instr: Instruction) =
        match instr with
        | x when instr.instr_type = I -> printI_Type x
        | x when instr.instr_type = I_O -> printI_O_Type x
        | x when instr.instr_type = I_S -> printI_SO_Type x
        | x when instr.instr_type = I_SO -> printI_SO_Type x
        | x when instr.instr_type = I_BO -> printI_BO_Type x
        | x when instr.instr_type = JJ -> printJ_Type x
        | x when instr.instr_type = R -> printR_Type x
        | x when instr.instr_type = R_V -> printR_V_Type x
        | x when instr.instr_type = R_S -> printR_S_Type x
        | x when instr.instr_type = R_J -> printR_J_Type x
        | x when instr.instr_type = R_M -> printR_M_Type x
        | _ -> failwith "Invalid Instruction!"

    /// Prints Parser Error message before ending program (NOT USED)
    let fail (msg: string) (line: int) =
        let msgs = msg.Split('\n')
        let found = msgs.[0].IndexOf(": ");
        let message = msgs.[0].Substring(found+2)
        printfn "Line %i: %s" line message
        failwith "Parser Error!" // Replace with whatever should come up in JS Console