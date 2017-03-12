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
    let parseI_Type (iTokens: string[]) =
        let opcode = Map.find iTokens.[0] IMap
        let r_s = Register(int(iTokens.[1]))
        let r_t = Register(int(iTokens.[2]))
        let immed = Half(uint16(iTokens.[3]))
        {opcode=opcode; instr_type = I; rs=r_s; rt=r_t; rd=Register(0); shift=Shiftval(0uy); immed=immed; target=Targetval(0u)}

    let parseJ_Type (jTokens: string[]) =            
        let opcode = Map.find jTokens.[0] JMap
        let target = Targetval(uint32(jTokens.[1]))
        {opcode=opcode; instr_type = J; rs=Register(0); rt=Register(0); rd=Register(0); shift=Shiftval(0uy); immed=Half(0us); target=target}

    let parseR_Type (rTokens: string[]) =            
        let opcode = Map.find rTokens.[0] RMap
        let r_s = Register(int(rTokens.[1]))
        let r_t = Register(int(rTokens.[2]))
        let r_d = Register(int(rTokens.[3]))
        let shift = Shiftval(byte(rTokens.[4]))
        {opcode=opcode; instr_type = R; rs=r_s; rt=r_t; rd=r_d; shift=shift; immed=Half(0us); target=Targetval(0u)}
    
    /// Parses Token into Instruction
    let parse (tokens: string[]) =
        if Map.containsKey tokens.[0] IMap then parseI_Type tokens
        elif Map.containsKey tokens.[0] JMap then parseJ_Type tokens
        elif Map.containsKey tokens.[0] RMap then parseR_Type tokens
        else failwith "Invalid Opcode: Does not exist in MIPS I!"

    // Print instruction helper functions
    let printI_Type (instr: Instruction) =
        printfn "Opcode: %A, $s: %A, $t: %A, imm: %A" instr.opcode instr.rs instr.rt instr.immed

    let printJ_Type (instr: Instruction) =
        printfn "Opcode: %A, target: %A" instr.opcode instr.target

    let printR_Type (instr: Instruction) =
        printfn "Opcode: %A, $s: %A, $t: %A, $d: %A, shift: %A" instr.opcode instr.rs instr.rt instr.rd instr.shift
    
    /// Prints parsed Instruction for debugging
    let printInstr (instr: Instruction) =
        match instr with
        | x when instr.instr_type = I -> printI_Type x
        | x when instr.instr_type = J -> printJ_Type x
        | x when instr.instr_type = R -> printR_Type x
        | _ -> failwith "Invalid Instruction!"