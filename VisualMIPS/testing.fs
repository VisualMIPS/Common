namespace VisualMIPS

open System
open System.Diagnostics
open Types

type TestCmd = 
    | GetState 
    | SetReg of uint32 * uint32 
    | RunInstr of uint32
    | Quit

type TestResults =
    | StateResult of Map<Register, Word> 

let getText (cmd: TestCmd) = match cmd with
    | GetState -> "s"
    | SetReg (a,b) -> "r "+(string a) + " " + (string b)
    | RunInstr a -> "i "+(string a)
    | Quit -> "q"


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