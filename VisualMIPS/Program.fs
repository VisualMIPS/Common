namespace VisualMIPS

open Types
open Instructions
open Parser
open Tokeniser
open MachineState
open MachineCode
open Executor
open Testing

module main =

    // Test Tokeniser and Parser
    (*
    let input = tokenise "AND 1, 2, 3"
    let AND = 
        try
            parse input
        with
            | msg -> fail (string msg) 1

    printfn "Instr: %A \n" AND
    printInstr AND
    printfn "Code: %u" (convert AND)
    
    let input1 = tokenise "JR 1"
    let JR = parse input1
    printfn "Instr: %A \n" JR
    printInstr JR

    let input2 = tokenise "JALR 1"
    let JALR = parse input2
    printfn "Instr: %A \n" JALR
    printInstr JALR

    let input3 = tokenise "JALR 1 2"
    let JALR1 = parse input3
    printfn "Instr: %A \n" JALR1
    printInstr JALR1

    let input4 = tokenise "SRA 1, 2, 16"
    let SRA = parse input4
    printfn "Instr: %A \n" SRA
    printInstr SRA

    let input5 = tokenise "SRAV 1, 2, 3"
    let SRAV = parse input5
    printfn "Instr: %A \n" SRAV
    printInstr SRAV

    let input6 = tokenise "BEQ 1, 2, 16"
    let BEQ = parse input6
    printfn "Instr: %A \n" BEQ
    printInstr BEQ

    let input7 = tokenise "BGEZ 1, 16"
    let BGEZ = parse input7
    printfn "Instr: %A \n" BGEZ
    printInstr BGEZ

    let input8 = tokenise "LB 1, 16(2)"
    let LB = parse input8
    printfn "Instr: %A \n" LB
    printInstr LB

    let input9 = tokenise "SLTI 1, 2, 16"
    let SLTI = parse input9
    printfn "Instr: %A \n" SLTI
    printInstr SLTI

    let input10 = tokenise "J 16"
    let J = parse input10
    printfn "Instr: %A \n" J
    printInstr J

    let input11 = tokenise "JAL 16"
    let JAL = parse input11
    printfn "Instr: %A \n" JAL
    printInstr JAL

    let input12 = tokenise "XOR 4, 2, 3"
    let XOR = parse input12
    printfn "Instr: %A \n" XOR
    printInstr XOR

    // Test Machine State Initialise and print
    initialise
    |> setReg (Register 3) (Word 32u)
    |> setReg (Register 2) (Word 32u)
    |> executeInstruction AND // Result is R1=32u
    |> executeInstruction XOR // Result is R4=0u
    |> printMachineState
    |> ignore

    System.Console.ReadKey() |> ignore
    //for testing, remember to test invalid inputs 
    // To do list : give each isntruction an address in memory in order to implement JR instruction
    // To do list : Create RuntimeErr in SetMap and GetMap when trying to access negative and inexisting addresses
    // To do list : Add Exceptions as Address Error for LWL and LWR
    *)
    printf "The best output: %A" (runTests ())