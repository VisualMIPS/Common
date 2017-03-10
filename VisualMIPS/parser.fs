namespace VisualMIPS

module Parser =

    open Types
    open Instructions

    // Placeholder functions from Tick4, will be replaced.
    // Need Error responses for different parsing situations
    type parseResponse =
        | VarValue of int // for commands that return data
        | ParseError // if command string is invalid
        | OK // for valid commads that return no data
    
    open System.Text.RegularExpressions
    // Need alpha (for function names) and num (for values) to identify valid tokens
    let isAlpha s = Regex.IsMatch (s, @"^[a-zA-Z]+$")

    let isNum s = Regex.IsMatch (s, @"^-?[0-9]+$")

    // Need parse function to take in tokens and output instruction type
    let parseI_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] IMap
        let r_s = Register(int(iTokens.[1]))
        let r_t = Register(int(iTokens.[2]))
        let immed = Half(uint16(iTokens.[3]))
        {opcode=opcode; r_s=r_s; r_t=r_t; immed=immed}

    let parseJ_Type (jTokens: string[]) =            
        let opcode = Map.find jTokens.[0] JMap
        let target = Targetval(uint32(jTokens.[1]))
        {opcode=opcode; target=target}

    let parseR_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] RMap
        let r_s = Register(int(rTokens.[1]))
        let r_t = Register(int(rTokens.[2]))
        let r_d = Register(int(rTokens.[3]))
        let shift = Shiftval(byte(rTokens.[4]))
        {opcode=opcode; r_s=r_s; r_t=r_t; r_d=r_d; shift=shift}

    let checkType (tokens: string[]) =
        if Map.containsKey tokens.[0] IMap then I (parseI_Type tokens)
        elif Map.containsKey tokens.[0] JMap then J (parseJ_Type tokens)
        elif Map.containsKey tokens.[0] RMap then R (parseR_Type tokens)
        else failwith "Invalid Opcode: Does not exist in MIPS I!"