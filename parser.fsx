(*  Tokeniser
    VisualMIPS 2017
*)

module Parser =
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
    // Need parse function to take in tokens and output function handle and data
    // Functions can be listed in map and indexed by name, e.g. Map<name of String, function> -> <"add", addFunction>
    let parse (tokens: string[]) =            
        match tokens.[0] with
        | "READ" when tokens.Length = 2 && isAlpha tokens.[1] -> readVar tokens.[1]
        | "WRITE" when tokens.Length = 3 && isAlpha tokens.[1] && isNum tokens.[2] -> writeVar tokens.[1] (int tokens.[2])
        | "ASSIGN" when tokens.Length = 3 && isAlpha tokens.[1] && isAlpha tokens.[2] -> assignVar tokens.[1] tokens.[2]
        | _ -> ParseError