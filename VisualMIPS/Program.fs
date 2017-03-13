namespace VisualMIPS

open Types
open Instructions
open Parser
open Tokeniser
open MachineState
open Executor

module main =
    // Test Tokeniser and Parser
    let input = tokenise "XOR 1, 2, 3, 0"
    let instr = parse input
    printfn "Instr: %A" instr

    // Test Machine State Initialise and print
    let mach = initialise
    let mach2 = setReg mach (Register 1) (Word 86u) 
    let mach3 = executeInstruction mach2 instr 
    printState mach3 |> ignore
    System.Console.ReadKey() |> ignore