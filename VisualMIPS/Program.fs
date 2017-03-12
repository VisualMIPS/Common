namespace VisualMIPS

open Types
open Instructions
open Parser
open Tokeniser
open MachineState

module main =
    // Test Tokeniser and Parser
    let input = tokenise "ADD 1, 2, 3, 0"
    printfn "Instr: %A" (parse input)

    // Test Machine State Initialise and print
    let mach = initialise
    printState mach |> ignore
    System.Console.ReadKey() |> ignore