namespace VisualMIPS

module Testing = 

    open System
    open System.Diagnostics
    open Types
    open Instructions
    open MachineCode
    open Executor

    type TestCmd = 
        | GetState 
        | SetReg of Register * Word 
        | RunInstr of Instruction
        | Quit

    type TestResults =
        | StateResult of Map<Register, Word>

    let getText (cmd: TestCmd) = 
        match cmd with
            | GetState -> "s"
            | SetReg (a,b) -> "r "+(string a) + " " + (string b)
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
        runProc "/Users/tom/Documents/Imperial/Year 3/HLP/Testing/golden/src/fyq14/test_mips" str

    let testAdd = 
        let i = {opcode=ADDU; instr_type = R; rs=Register(3); rt=Register(4); rd=Register(5); shift=Shiftval(0uy); immed=Half(0us); target=Targetval(0u)}
        let goldenResult = runList [SetReg (Register 3, 42u);SetReg (Register 4, 9u);RunInstr i; GetState; Quit]
        let mach = 