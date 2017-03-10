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
    let parse (tokens: string[]) =            
        let opcode = tokens.[0]
        let r_s = uint32(tokens.[1])
        let r_t = uint32(tokens.[2])
        let r_d = uint32(tokens.[3])
        let shift = byte(tokens.[4])
        {opcode; r_s; r_t; r_d; shift}