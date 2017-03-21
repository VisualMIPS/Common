namespace VisualMIPS

module MachineState =

    open Types

    type RunState = 
        | RunOK
        | RunTimeErr of string
        | SyntaxErr of string
        
    type MachineState = 
        { 
        RegMap : Map<Register, Word> //32 Registers
        Hi : Word
        Lo : Word
        MemMap : Map<Memory, Byte> // 4096 memory address although Memory is uint32
        State : RunState
        pc : Word //The instruction currently running
        pcNext : Word //The address of the next instruction to run
        pcNextNext : Word option //The address of the instruction to run after that
        }

    /// Gets value of specified Register
    let getReg (reg: Register) (mach: MachineState) =
        Map.find reg mach.RegMap
    
    /// Gets value of High Register
    let getHi (mach: MachineState) =
        mach.Hi
       
    /// Gets value of Low Register
    let getLo (mach: MachineState) =
        mach.Lo

    /// Gets value of specified Memory location
    let getMem (mem: Memory) (mach: MachineState) =
        match (T.getValue(mem) < 4096u) with
        | true -> Map.find mem mach.MemMap
        | false -> failwithf "RunTimeErr : unexisting memory address with function getMem"
        

    /// Gets current State value
    let getState (mach: MachineState) =
        mach.State

    /// Gets current PC value
    let getPC (mach: MachineState) =
        mach.pc

    /// Gets next PC value
    let getNextPC (mach: MachineState) =
        mach.pcNext
    
    /// Gets next, next PC value
    let getNextNextPC (mach: MachineState) =
        mach.pcNextNext

    let advancePC (mach: MachineState) =
        let m = {mach with pc = mach.pcNext}
        let m2 = 
            match mach.pcNextNext with
            |Some nnext -> {mach with pcNext = nnext}
            |None -> {mach with pcNext = Word (T.getValue(mach.pcNext)+4u)}
        {m2 with pcNextNext = None}

    /// Sets next, next PC value
    let setNextNextPC (next: Word) (mach: MachineState) =
        {mach with pcNextNext = Some next}

    /// Sets value into specified Register. If Register is 0, nothing is changed.
    let setReg (reg: Register) (data: Word) (mach: MachineState) =
        if reg = Register(0) then mach  // If reg = 0, return unaltered mach as r0 is always 0.
        else
            let newRegMap = Map.add reg data mach.RegMap
            let newMach = {mach with RegMap = newRegMap}
            newMach

    /// Sets value into High Register
    let setHi (data: Word) (mach: MachineState) =
        let newMach = {mach with Hi = data}
        newMach

    /// Sets value into Low Register
    let setLo (data: Word)  (mach: MachineState)=
        let newMach = {mach with Lo = data}
        newMach
    
    /// Sets value into specified Memory location
    let setMem (mem: Memory) (data: Byte) (mach: MachineState) =
        match (T.getValue(mem) < 4096u) with
        | true -> 
            let newMemMap = Map.add mem data mach.MemMap
            let newMach = {mach with MemMap = newMemMap}
            newMach
        | false -> failwithf "RunTimeErr : unexisting memory address with function setMem"
        
    
    /// Sets value into State (RunState)
    let setState (state: RunState) (mach: MachineState) =
        let newMach = {mach with State = state}
        newMach

    /// Prints entire Machine State
    let printMachineState (mach: MachineState) =
        printfn "Current Machine State:"
        printfn "PC: \t\t%A" mach.pc
        printfn "PCNext: \t%A" mach.pcNext
        printfn "PCNextNext: \t%A" mach.pcNextNext
        printfn "State: \t\t%A" mach.State

        printfn "Registers:"
        for i in 0..31 do
            printfn "\t\tR%A:\t%A" i mach.RegMap.[Register(i)]
        printfn "\t\tHigh:\t%A" mach.Hi
        printfn "\t\tLow:\t%A" mach.Lo

        printfn "Memory: %A" mach.MemMap
    
    /// Initialises Machine State at start of program
    let initialise =
        let regMap =
            let reg = [|0..31|]
            reg |> Array.map (fun i -> (Register(i), Word(0u))) |> Map.ofArray
        let memMap =
            let memSpace = [|0u..4095u|]
            memSpace |> Array.map (fun i -> ( Memory(i), (Byte (byte 0)) ) ) |> Map.ofArray
        let megaRecord ={RegMap=regMap; Hi=Word(0u); Lo=Word(0u); MemMap=memMap; State=RunOK; pc=Word(0u); pcNext=Word(4u); pcNextNext=None}
        megaRecord

