namespace VisualMIPS

module MachineState =

    open Types

    type RunState = 
        | RunOK
        | RunTimeErr of string
        | SyntaxErr of string
        
    type MachineState = 
        { 
        RegMap : Map<Register, Word>
        Hi : Word
        Lo : Word
        MemMap : Map<Memory, Word> 
        State : RunState
        pc : Word
        pcNext : Word
        }

    /// Gets value of specified Register
    let getReg (reg: Register) (mach:MachineState) =
        Map.find reg mach.RegMap
    
    /// Gets value of High Register
    let getHi (mach: MachineState) =
        mach.Hi
       
    /// Gets value of Low Register
    let getLo (mach: MachineState) =
        mach.Lo

    /// Gets value of specified Memory location
    let getMem (mem: Memory) (mach:MachineState) =
        Map.find mem mach.MemMap

    
    /// Gets current PC value
    let getPC (mach:MachineState) =
        mach.pc

    /// Gets next PC value
    let getNextPC (mach:MachineState) =
        mach.pcNext

    /// Sets value into specified Register
    let setReg (reg: Register) (data: Word) (mach:MachineState) =
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
    let setMem (mem: Memory) (data: Word) (mach:MachineState) =
        let newMemMap = Map.add mem data mach.MemMap
        let newMach = {mach with MemMap = newMemMap}
        newMach

    /// Prints entire Machine State
    let printState (mach:MachineState) =
        printfn "Current Machine State:"
        printfn "PC: \t\t%A" mach.pc
        printfn "PCNext: \t%A" mach.pcNext
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

        let memMap = Map.empty

        {RegMap=regMap; Hi=Word(0u); Lo=Word(0u); MemMap=memMap; State=RunOK; pc=Word(0u); pcNext=Word(4u)}
            
(* // Fronm C compiler -> keeps memory of clock cycles or smg, ct remember
  void advance_pc (SWORD offset)
{
    PC  =  nPC;
   nPC  += offset;
} 
*)
