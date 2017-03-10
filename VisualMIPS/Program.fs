namespace VisualMIPS

open Types
open Instructions
open Parser
open Tokeniser

module main =
    let input = tokenise "ADD 1, 2, 3, 0"
    printfn "Instr: %A" (checkType input)
    System.Console.ReadKey() |> ignore