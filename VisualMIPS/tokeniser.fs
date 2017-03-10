namespace VisualMIPS

module Tokeniser =

    // Need tokenise function to return tokens to parse. Simple split string into components
    let tokenise (s: string) = 
        Array.filter ((<>) "") (s.Split(' ',',','\t','\n','\r','\f'))

    // Helper function in case web input is one large string and outputs array of strings representing new lines
    let split (input: string) =
        Array.filter ((<>) "") (input.Split('\n','\r','\f'))

    // Tokeniser does not handle error checking. This is done by the parser when it checks each parameter.