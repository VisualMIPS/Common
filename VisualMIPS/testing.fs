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
        | RunInstr of Instruction
        | Quit

    type TestResults =
        | StateResult of Map<Register, Word>

    let getText (cmd: TestCmd) = 
        match cmd with
            | GetState -> "s"
            | RegSet (a,b) -> "r "+(a |> T.getValue |> string) + " " + (b |> T.getValue |> string)
            | RunInstr i -> "i "+(i |> convert |> string)
            | Quit -> "q"

    let executeLocally cmd mach =
        match cmd with
            | GetState -> (mach.RegMap |> StateResult |> Some, mach)
            | RegSet (a,b) -> (None, setReg a b mach)
            | RunInstr i -> (None, executeInstruction i mach)
            | Quit -> (None, mach)

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
        p.StandardInput.WriteLine(textIn)
        p.BeginOutputReadLine()
        p.BeginErrorReadLine()
        p.WaitForExit()
        let cleanOut l = l |> Seq.filter (fun o -> String.IsNullOrEmpty o |> not)
        cleanOut outputs

    let runList ls = 
        let strs = List.map getText ls
        let str = strs |> List.fold (fun x y -> x + y + "\n") ""
        runProc "/Users/tom/Documents/Imperial/Year 3/HLP/Common/VisualMIPS/golden-mips/src/fyq14/test_mips" str
    
    let runCmdsLocal cmds =
        let foldfn (outputs, m) c = 
            let o, nextM = executeLocally c m
            (List.append outputs [o], nextM)
        let z = List.fold foldfn ([],initialise) cmds
        z |> fst |> List.choose id
    
    let runCmdsGolden commands =
        let txt = runList commands
        let rec getResults cmds (textout: seq<string>)  =
            let processRegLine (x: string) = 
                let itms = Array.map uint32 (x.Split([|' '|]))
                (itms.[0] |> int |> Register,itms.[1] |> Word)
            match cmds with
                | GetState :: xs -> (textout |> Seq.toList |> List.take 32 |> List.map processRegLine |> Map.ofList |> StateResult) :: getResults xs (Seq.skip 32 textout)
                | _ :: xs -> getResults xs textout
                | [] -> []
        getResults commands txt

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
        let edgecases = [0x00000000u; 0x00000001u; 0xFFFFFFFFu; 0x7FFFFFFFu; 0x80000000u; 0x0000FFFFu; 0x0000000Fu; 0x000000FFu; 0x0000001Fu]
        if randbool () then
            randChoice edgecases //50% chance of picking an edge case, 50% chance of picking a totally random value
        else randuint ()
    let randRegMap () =
        [0..31] |> List.map (fun x -> (Register x, regVal () |> Word)) |> Map.ofList |> Map.add (Register 0) (Word 0u)
    let randReg () = rng.Next(0,32) |> Register
    let cmdsFromRegMap rmap =
        rmap |> Map.toList |> List.map RegSet
    let emptyInstr = {opcode=ADDU; instr_type = R; rs=Register 0; rt=Register 0; rd=Register 0; shift=Shiftval(0uy); immed=Half(0us); target=Targetval(0u)}

    let printDiffs (mp: Map<Register,Word>) (golden: Map<Register,Word>) = 
        let comp x = mp.[Register x] = golden.[Register x]
        let prnt x = printfn "Register %A Diff: We got %A vs golden %A" x mp.[Register x] golden.[Register x]
        List.map (fun x -> if comp x then () else prnt x) [0..31] |> ignore
        ()

    let multiRun runner oc = List.fold (fun x y-> runner oc && x) true [0..500] |> ignore ;printfn "Opcode complete: %A" oc

    let printError msg cmds=
        printf "%s" (String.replicate 30 "=")
        printf "%s" msg
        printfn "%s" (String.replicate 30 "=")
        printfn "Commands that caused error:"
        printfn "%A" cmds
    
    let printDiff us gold n =
        if us=gold then printfn "Output %A matches. " n
        else match us,gold with
                |StateResult u, StateResult g -> printfn "Output %A does not match:" n; printDiffs u g
                | _,_ -> printfn "Strange bugs are afoot." //For later on when we have multiple types


    let runTest cmds=
        let supress = true //Supress output for debugging

        let ourOutput = try runCmdsLocal cmds |> Some
                        with x -> printError "EXCEPTION IN OUR CODE" cmds; printfn "%A" x; None
        let goldenOutput = try runCmdsGolden cmds |> Some
                            with x -> printError "EXCEPTION IN GOLDEN TESTER" cmds; printfn "%A" x; None
        match ourOutput, goldenOutput with
            | None, _ | _, None -> false //If an exception has occured this is obviously a failure
            | Some x, Some y when x=y -> true
            | Some us, Some gol when not supress-> printError "OUTPUTS DID NOT MATCH" cmds; List.map (fun x -> printDiff us.[x] gol.[x] x) [0..(List.length us - 1)] |> ignore; false
            | _,_ -> false //When output is supressed
    let testR oc =
        let i = {opcode=oc; instr_type = R; rs=randReg (); rt=randReg (); rd=randReg (); shift=Shiftval(0uy); immed=Half(0us); target=Targetval(0u)}
        let rMap = randRegMap ()
        let cmds = List.append (cmdsFromRegMap rMap) [RunInstr i; GetState; Quit]
        runTest cmds

    let testM oc =
        let testMult = {emptyInstr with opcode=oc; instr_type = R_M; rs=randReg (); rt=randReg()}
        let mfhi = {emptyInstr with opcode=MFHI; instr_type = R_J; rd=randReg ()}
        let mflo = {emptyInstr with opcode=MFLO; instr_type = R_J; rd=randReg ()}
        let rMap = randRegMap ()
        let cmds = List.append (cmdsFromRegMap rMap) [RunInstr testMult;RunInstr mfhi;RunInstr mflo; GetState; Quit]
        runTest cmds
    
    let testR_S oc = 
        let i = {opcode=oc; instr_type = R_S; rs=Register 0; rt=randReg (); rd=randReg (); shift=Shiftval(rng.Next(0,32)|> byte); immed=Half(0us); target=Targetval(0u)}
        let rMap = randRegMap ()
        let cmds = List.append (cmdsFromRegMap rMap) [RunInstr i; GetState; Quit]
        runTest cmds

    let testI oc =
        let i = {opcode=oc; instr_type = I; rs=randReg (); rt=randReg (); rd=Register 0; shift=Shiftval(0uy); immed=Half(rng.Next(0,1<<<16)|> uint16); target=Targetval(0u)}
        let rMap = randRegMap ()
        let cmds = List.append (cmdsFromRegMap rMap) [RunInstr i; GetState; Quit]
        runTest cmds
    (*
    let testSETHILO oc =
        let testMult = {emptyInstr with opcode=MTHI; instr_type = R_M; rs=randReg (); rt=randReg()}
        let mfhi = {emptyInstr with opcode=MFHI; instr_type = R_J; rd=randReg ()}
        let mflo = {emptyInstr with opcode=MFLO; instr_type = R_J; rd=randReg ()}
        let rMap = randRegMap ()
        let cmds = List.append (cmdsFromRegMap rMap) [RunInstr testMult;RunInstr mfhi;RunInstr mflo; GetState; Quit]
    *)
    let runMany n f m = 
        let outputs = List.map (fun x -> f ()) [1..n]
        printfn "For tests on %A :: %A" m (List.countBy id outputs)

    let runTests () =
        let n = 100
        let rm f oc = runMany n (fun () -> f oc) oc
        let r_ocs = [ADDU;AND;OR;SUBU;XOR;SLT;SLTU;SRAV;SRLV;SLLV]
        let r_socs = [SRA; SLL; SRL]
        let m_ocs = [DIV; DIVU; MULT; MULTU]
        let i_ocs = [ANDI; ORI; XORI; SLTI; SLTIU; ADDIU]

        List.map (rm testR) r_ocs |> ignore
        List.map (rm testM) m_ocs |> ignore
        List.map (rm testR_S) r_socs |> ignore
        List.map (rm testI) i_ocs |> ignore