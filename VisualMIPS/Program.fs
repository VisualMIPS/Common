namespace VisualMIPS

open Types
open Instructions
open Parser
open Tokeniser
open MachineState
open Executor

module main =
    // Test Tokeniser and Parser
    let input = tokenise "AND 1, 2, 3, 0"
    let instr = parse input
    printfn "Instr: %A \n" instr 

    // Test Machine State Initialise and print
    initialise
    |> setReg (Register 1) (Word 32u)
    |> setReg (Register 2) (Word 32u)
    |> executeInstruction instr // Result is 32u
    |> printState
    |> ignore

    System.Console.ReadKey() |> ignore
    
    //for testing, remember to test invalid inputs 
    