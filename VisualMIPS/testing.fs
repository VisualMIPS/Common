namespace VisualMIPS

module Testing = 

    open System
    open System.Diagnostics
    open Types
    open Instructions
    open MachineCode
    open Executor
    open MachineState

    type TestCmd = 
        | GetState 
        | RegSet of Register * Word
        | HISet of Word
        | LOSet of Word
        | RunInstr of Instruction
        | Quit

    type TestResults =
        | StateResult of Map<Register, Word>

    let getText (cmd: TestCmd) = 
        match cmd with
            | GetState -> "s"
            | RegSet (a,b) -> "r "+(a |> T.getValue |> string) + " " + (b |> T.getValue |> string)
            | HISet a -> "h " + (a |> T.getValue |> string)
            | LOSet a -> "l " + (a |> T.getValue |> string)
            | RunInstr i -> "i "+(i |> convert |> string)
            | Quit -> "q"

    
    let rec getResults cmds (textout: seq<string>)  =
        let processRegLine (x: string) = 
            let itms = Array.map uint32 (x.Split([|' '|]))
            (itms.[0] |> int |> Register,itms.[1] |> Word)
        match cmds with
            | GetState :: xs -> (textout |> Seq.toList |> List.take 32 |> List.map processRegLine |> Map.ofList |> StateResult) :: getResults xs (Seq.skip 32 textout)
            | _ :: xs -> getResults xs textout
            | [] -> []


    let runProc filename (textIn : string): seq<string> = 
        let procStartInfo = 
            ProcessStartInfo(
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                FileName = filename,
                Arguments = ""
            )
        
        let outputs = System.Collections.Generic.List<string>()
        let errors = System.Collections.Generic.List<string>()
        let outputHandler f (_sender:obj) (args:DataReceivedEventArgs) = f args.Data
        use p = new Process(StartInfo = procStartInfo)
        p.OutputDataReceived.AddHandler(DataReceivedEventHandler (outputHandler outputs.Add))
        p.ErrorDataReceived.AddHandler(DataReceivedEventHandler (outputHandler errors.Add))
        let started = 
            try
                p.Start()
            with | ex ->
                ex.Data.Add("filename", filename)
                reraise()
        if not started then
            failwithf "Failed to start process %s" filename
        printfn "Started %s with pid %i" p.ProcessName p.Id
        p.StandardInput.WriteLine(textIn)
        p.BeginOutputReadLine()
        p.BeginErrorReadLine()
        p.WaitForExit()
        printfn "Finished %s" filename 
        let cleanOut l = l |> Seq.filter (fun o -> String.IsNullOrEmpty o |> not)
        cleanOut outputs
        //runProc "/Users/tom/Documents/Imperial/Year 3/HLP/Testing/printHello.py" "banter\nlol";;
        //runProc "/Users/tom/Documents/Imperial/Year 3/HLP/Testing/golden/src/fyq14/test_mips" "s\nq";;
        ///


    let runList ls = 
        let strs = List.map getText ls
        let str = strs |> List.fold (fun x y -> x + y + "\n") ""
        printf "%A" str |> ignore
        runProc "/Users/tom/Documents/Imperial/Year 3/HLP/Common/VisualMIPS/golden-mips/src/fyq14/test_mips" str
    
    let getStateDONTUSE s =
        match s with
        | StateResult m -> m

    let rng = System.Random()

    let randChoice s = 
        let len = List.length s
        s.[rng.Next(0,len)]
    
    let randuint () = 
        let a = rng.Next(1<<<30) |> uint32
        let b = rng.Next(1<<<2) |> uint32
        (a <<< 2) ||| b
    
    let randbool () =
        match rng.Next(0,2) with
            |0 -> false
            |_ -> true
    let regVal () =
        let edgecases = [0x00000000u; 0x00000001u; 0xFFFFFFFFu; 0xF0F0F0F0u; 0x7FFFFFFFu; 0x80000000u; 0xDEADBEEFu]
        if randbool () then
            randChoice edgecases
        else randuint ()

    let randRegMap () =
        [0..31] |> List.map (fun x -> (Register x, regVal () |> Word)) |> Map.ofList |> Map.add (Register 0) (Word 0u)
        
    let randReg () = rng.Next(0,32) |> Register

    let cmdsFromRegMap rmap =
        rmap |> Map.toList |> List.map RegSet

    let emptyInstr = {opcode=ADDU; instr_type = R; rs=Register 0; rt=Register 0; rd=Register 0; shift=Shiftval(0uy); immed=Half(0us); target=Targetval(0u)}

    let testHILOs =
        //let mthi = {emptyInstr with opcode=MTHI; instr_type = R_J; rs=randReg ()}
        //let mtlo = {emptyInstr with opcode=MTLO; instr_type = R_J; rs=randReg ()}
        let testMult = {emptyInstr with opcode=MULT; instr_type = R_M; rs=randReg (); rt=randReg()}
        let mfhi = {emptyInstr with opcode=MFHI; instr_type = R_J; rd=randReg ()}
        let mflo = {emptyInstr with opcode=MFLO; instr_type = R_J; rd=randReg ()}
        let rMap = randRegMap ()
        let cmds = List.append (cmdsFromRegMap rMap) [RunInstr testMult;RunInstr mfhi;RunInstr mflo; GetState; Quit]
        let goldenResult = runList cmds
        let mach = {initialise with RegMap = rMap} |> executeInstruction testMult |> executeInstruction mfhi |> executeInstruction mflo
        mach.RegMap = (goldenResult |> getResults cmds |> List.head |> getStateDONTUSE)
        
    let testAdd = 
        let i = {opcode=ADDU; instr_type = R; rs=randReg (); rt=randReg (); rd=randReg (); shift=Shiftval(0uy); immed=Half(0us); target=Targetval(0u)}
        let rMap = randRegMap ()
        let cmds = List.append (cmdsFromRegMap rMap) [RunInstr i; GetState; Quit]
        let goldenResult = runList cmds
        let mach = {initialise with RegMap = rMap} |> executeInstruction i
        mach.RegMap = (goldenResult |> getResults cmds |> List.head |> getStateDONTUSE)