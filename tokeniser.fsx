(*  Tokeniser
    VisualMIPS 2017
*)

module Tokeniser =
    // Placeholder functions from Tick4, will be replaced.
    // Need Error responses for different situations.
    type tokenResponse =
        | VarValue of int // for commands that return data
        | TokeniseError // if command string is invalid
        | OK // for valid commads that return no data
    // Need tokenise function to return tokens to parse
    let tokenise (s: string) = 
        Array.filter ((<>) "") (s.Split(' ' , '\t','\n','\r','\f'))  