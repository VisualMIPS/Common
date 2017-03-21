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
    *)
    open System.Text.RegularExpressions
    // Need alpha (for function names) and num (for values) to identify valid tokens
    //let isAlpha s = Regex.IsMatch (s, @"^[a-zA-Z]+$")

    let isNum s = Regex.IsMatch (s, @"^-?[0-9]+$")

    let regWithinRange (reg: int) =    match reg with
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

        if iTokens.Length <> 4 then failwithf "Invalid Operation: %A. Takes 3 parameters." iTokens.[0]

        if not (isNum iTokens.[1]) then failwithf "rt: %A is invalid. Please use integers only." iTokens.[1]
        if not (regWithinRange (int iTokens.[1])) then failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." (int iTokens.[1])
        let r_t = iTokens.[1] |> int |> Register

        if not (isNum iTokens.[2]) then failwithf "rs: %A is invalid. Please use integers only." iTokens.[2]
        if not (regWithinRange (int iTokens.[2])) then failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." (int iTokens.[2])
        let r_s = iTokens.[2] |> int |> Register

        if not (isNum iTokens.[3]) then failwithf "imm: %A is invalid. Please use integers only." iTokens.[3]
        if not (immWithinRange (int iTokens.[3])) then failwithf "imm: %A is not within range. Accepted values between -32768 and 32767." (int iTokens.[3])
        let immed = iTokens.[3] |> uint16 |> Half

        {opcode=opcode; instr_type = I; rs=r_s; rt=r_t; rd=Register 0; shift=Shiftval 0uy; immed=immed; target=Targetval 0u}

    /// Parse (Opcode rs, rt, offset)
    let parseI_O_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_OMap

        if iTokens.Length <> 4 then failwithf "Invalid Operation: %A. Takes 3 parameters." iTokens.[0]

        if not (isNum iTokens.[1]) then failwithf "rs: %A is invalid. Please use integers only." iTokens.[1]
        if not (regWithinRange (int iTokens.[1])) then failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." (int iTokens.[1])
        let r_s = iTokens.[1] |> int |> Register

        if not (isNum iTokens.[2]) then failwithf "rt: %A is invalid. Please use integers only." iTokens.[2]
        if not (regWithinRange (int iTokens.[2])) then failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." (int iTokens.[2])
        let r_t = iTokens.[2] |> int |> Register

        if not (isNum iTokens.[3]) then failwithf "offset: %A is invalid. Please use integers only." iTokens.[3]
        if not (immWithinRange (int iTokens.[3])) then failwithf "offset: %A is not within range. Accepted values between -32768 and 32767." (int iTokens.[3])
        let immed = iTokens.[3] |> uint16 |> Half

        {opcode=opcode; instr_type = I_O; rs=r_s; rt=r_t; rd=Register 0; shift=Shiftval 0uy; immed=immed; target=Targetval 0u}

    /// Parse (Opcode rs, offset) Same as I_SO
    let parseI_S_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_SMap

        if iTokens.Length <> 3 then failwithf "Invalid Operation: %A. Takes 2 parameters." iTokens.[0]
        
        if not (isNum iTokens.[1]) then failwithf "rs: %A is invalid. Please use integers only." iTokens.[1]
        if not (regWithinRange (int iTokens.[1])) then failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." (int iTokens.[1])
        let r_s = iTokens.[1] |> int |> Register

        if not (isNum iTokens.[2]) then failwithf "offset: %A is invalid. Please use integers only." iTokens.[2]
        if not (immWithinRange (int iTokens.[2])) then failwithf "offset: %A is not within range. Accepted values between -32768 and 32767." (int iTokens.[2])
        let immed = iTokens.[2] |> uint16 |> Half

        {opcode=opcode; instr_type = I_S; rs=r_s; rt=Register 0; rd=Register 0; shift=Shiftval 0uy; immed=immed; target=Targetval 0u}
    
    /// Parse (Opcode rs, offset) Same as I_S
    let parseI_SO_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_SOMap
        
        if not (isNum iTokens.[1]) then failwithf "rs: %A is invalid. Please use integers only." iTokens.[1]
        if not (regWithinRange (int iTokens.[1])) then failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." (int iTokens.[1])
        let r_t, r_s =
            match opcode with
            | LUI when iTokens.Length = 2 -> (iTokens.[1] |> int |> Register), Register 0
            | LUI when iTokens.Length <> 2 -> failwithf "Invalid Operation: %A. Takes 1 parameter." iTokens.[0]
            | _ when iTokens.Length = 3 -> Register 0, (iTokens.[1] |> int |> Register)
            | _ -> failwithf "Invalid Operation: %A. Takes 2 parameters." iTokens.[0]

        if not (isNum iTokens.[2]) then failwithf "offset: %A is invalid. Please use integers only." iTokens.[2]
        if not (immWithinRange (int iTokens.[2])) then failwithf "offset: %A is not within range. Accepted values between -32768 and 32767." (int iTokens.[2])
        let immed = iTokens.[2] |> uint16 |> Half

        {opcode=opcode; instr_type = I_SO; rs=r_s; rt=r_t; rd=Register 0; shift=Shiftval 0uy; immed=immed; target=Targetval 0u}
    
    /// Parse (Opcode rt, offset(rs))
    let parseI_BO_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] I_BOMap

        if iTokens.Length <> 4 then failwithf "Invalid Operation: %A. Takes 3 parameters." iTokens.[0]
        
        if not (isNum iTokens.[1]) then failwithf "rt: %A is invalid. Please use integers only." iTokens.[1]
        if not (regWithinRange (int iTokens.[1])) then failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." (int iTokens.[1])
        let r_t = iTokens.[1] |> int |> Register

        if not (isNum iTokens.[2]) then failwithf "offset: %A is invalid. Please use integers only." iTokens.[2]
        if not (immWithinRange (int iTokens.[2])) then failwithf "offset: %A is not within range. Accepted values between -32768 and 32767." (int iTokens.[2])
        let immed = iTokens.[2] |> uint16 |> Half

        if not (isNum iTokens.[3]) then failwithf "rs: %A is invalid. Please use integers only." iTokens.[3]
        if not (regWithinRange (int iTokens.[3])) then failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." (int iTokens.[3])
        let r_s = iTokens.[3] |> int |> Register

        {opcode=opcode; instr_type = I_BO; rs=r_s; rt=r_t; rd=Register 0; shift=Shiftval 0uy; immed=immed; target=Targetval 0u}

    /// Parse (Opcode target)
    let parseJ_Type (jTokens: string[]) =            
        let opcode = Map.find jTokens.[0] JMap

        if jTokens.Length <> 2 then failwithf "Invalid Operation: %A. Takes 1 parameter." jTokens.[0]

        if not (isNum jTokens.[1]) then failwithf "target: %A is invalid. Please use integers only." jTokens.[1]
        if not (targetWithinRange (int jTokens.[1])) then failwithf "target: %A is not within range. Accepted values between 0 and 67108863." (int jTokens.[1])
        let target = jTokens.[1] |> uint32 |> Targetval

        {opcode=opcode; instr_type = JJ; rs=Register 0; rt=Register 0; rd=Register 0; shift=Shiftval 0uy; immed=Half 0us; target=target}

    /// Parse (Opcode rd, rs, rt)
    let parseR_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] RMap

        if rTokens.Length <> 4 then failwithf "Invalid Operation: %A. Takes 3 parameters." rTokens.[0]

        if not (isNum rTokens.[1]) then failwithf "rd: %A is invalid. Please use integers only." rTokens.[1]
        if not (regWithinRange (int rTokens.[1])) then failwithf "rd: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[1])
        let r_d = rTokens.[1] |> int |> Register

        if not (isNum rTokens.[2]) then failwithf "rs: %A is invalid. Please use integers only." rTokens.[2]
        if not (regWithinRange (int rTokens.[2])) then failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[2])
        let r_s = rTokens.[2] |> int |> Register

        if not (isNum rTokens.[3]) then failwithf "rt: %A is invalid. Please use integers only." rTokens.[3]
        if not (regWithinRange (int rTokens.[3])) then failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[3])
        let r_t = rTokens.[3] |> int |> Register

        {opcode=opcode; instr_type = R; rs=r_s; rt=r_t; rd=r_d; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}
    
    /// Parse (Opcode rd, rt, rs)
    let parseR_V_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] R_VMap

        if rTokens.Length <> 4 then failwithf "Invalid Operation: %A. Takes 3 parameters." rTokens.[0]

        if not (isNum rTokens.[1]) then failwithf "rd: %A is invalid. Please use integers only." rTokens.[1]
        if not (regWithinRange (int rTokens.[1])) then failwithf "rd: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[1])
        let r_d = rTokens.[1] |> int |> Register

        if not (isNum rTokens.[2]) then failwithf "rt: %A is invalid. Please use integers only." rTokens.[2]
        if not (regWithinRange (int rTokens.[2])) then failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[2])
        let r_t = rTokens.[2] |> int |> Register

        if not (isNum rTokens.[3]) then failwithf "rs: %A is invalid. Please use integers only." rTokens.[3]
        if not (regWithinRange (int rTokens.[3])) then failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[3])
        let r_s = rTokens.[3] |> int |> Register
        
        {opcode=opcode; instr_type = R_V; rs=r_s; rt=r_t; rd=r_d; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}

    /// Parse (Opcode rd, rt, shift)
    let parseR_S_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] R_SMap

        if rTokens.Length <> 4 then failwithf "Invalid Operation: %A. Takes 3 parameters." rTokens.[0]

        if not (isNum rTokens.[1]) then failwithf "rd: %A is invalid. Please use integers only." rTokens.[1]
        if not (regWithinRange (int rTokens.[1])) then failwithf "rd: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[1])
        let r_d = rTokens.[1] |> int |> Register

        if not (isNum rTokens.[2]) then failwithf "rt: %A is invalid. Please use integers only." rTokens.[2]
        if not (regWithinRange (int rTokens.[2])) then failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[2])
        let r_t = rTokens.[2] |> int |> Register

        if not (isNum rTokens.[3]) then failwithf "shift: %A is invalid. Please use integers only." rTokens.[3]
        if not (regWithinRange (int rTokens.[3])) then failwithf "shift: %A is not within range. Accepted shift values between 0 and 31." (int rTokens.[3])
        let shift = rTokens.[3] |> byte |> Shiftval
        {opcode=opcode; instr_type = R_S; rs=Register 0; rt=r_t; rd=r_d; shift=shift; immed=Half 0us; target=Targetval 0u}

    /// Parse (Opcode rd, rs) or (Opcode rs) or (Opcode rd)
    let parseR_J_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] R_JMap
        if opcode = JALR then
            if rTokens.Length = 3 then
                if not (isNum rTokens.[1]) then failwithf "rd: %A is invalid. Please use integers only." rTokens.[1]
                if not (regWithinRange (int rTokens.[1])) then failwithf "rd: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[1])
                let r_d = rTokens.[1] |> int |> Register

                if not (isNum rTokens.[2]) then failwithf "rs: %A is invalid. Please use integers only." rTokens.[2]
                if not (regWithinRange (int rTokens.[2])) then failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[2])
                let r_s = rTokens.[2] |> int |> Register

                {opcode=opcode; instr_type = R_J; rs=r_s; rt=Register 0; rd=r_d; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}
            elif rTokens.Length = 2 then
                let r_d = Register(31)

                if not (isNum rTokens.[1]) then failwithf "rs: %A is invalid. Please use integers only." rTokens.[1]
                if not (regWithinRange (int rTokens.[1])) then failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[1])
                let r_s = rTokens.[1] |> int |> Register

                {opcode=opcode; instr_type = R_J; rs=r_s; rt=Register 0; rd=r_d; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}
            
            else failwithf "Invalid Operation: %A. Takes 1 or 2 parameters." rTokens.[0]

        elif rTokens.Length = 2 then
            if opcode = MFLO || opcode = MFHI then
                if not (isNum rTokens.[1]) then failwithf "rs: %A is invalid. Please use integers only." rTokens.[1]
                if not (regWithinRange (int rTokens.[1])) then failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[1])
                let r_d = rTokens.[1] |> int |> Register

                {opcode=opcode; instr_type = R_J; rs=Register 0; rt=Register 0; rd=r_d; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}
        
            else
                if not (isNum rTokens.[1]) then failwithf "rs: %A is invalid. Please use integers only." rTokens.[1]
                if not (regWithinRange (int rTokens.[1])) then failwithf "rs: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[1])
                let r_s = rTokens.[1] |> int |> Register

                {opcode=opcode; instr_type = R_J; rs=r_s; rt=Register 0; rd=Register 0; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}
        
        else failwithf "Invalid Operation: %A. Takes 1 parameter." rTokens.[0]

    /// Parse (Opcode rs, rt)
    let parseR_M_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] R_MMap

        if rTokens.Length <> 3 then failwithf "Invalid Operation: %A. Takes 2 parameters." rTokens.[0]

        if not (isNum rTokens.[1]) then failwithf "rs: %A is invalid. Please use integers only." rTokens.[1]
        if not (regWithinRange (int rTokens.[1])) then failwithf "rd: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[1])
        let r_s = rTokens.[1] |> int |> Register

        if not (isNum rTokens.[2]) then failwithf "rt: %A is invalid. Please use integers only." rTokens.[2]
        if not (regWithinRange (int rTokens.[2])) then failwithf "rt: %A is not within range. Accepted Registers between 0 and 31." (int rTokens.[2])
        let r_t = rTokens.[2] |> int |> Register

        {opcode=opcode; instr_type = R_M; rs=r_s; rt=r_t; rd=Register 0; shift=Shiftval 0uy; immed=Half 0us; target=Targetval 0u}

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
        elif Map.containsKey tokens.[0] R_MMap then parseR_M_Type tokens
        else failwithf "Syntax Error! \nInvalid Operation: %A does not exist in MIPS I! \nIs your operation name all UPPERCASE?" tokens.[0]

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