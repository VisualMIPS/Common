namespace VisualMIPS

module MachineState =

    open Types

    type RunState = 
        | RunOK
        | RunTimeErr of string
        | SyntaxErr of string
        
    type MachineState = 
        { 
        RegMap : Map<Register, uint32>
        Hi : uint32
        Lo : uint32
        MemMap : Map<Memory, uint32> 
        State : RunState
        pc : uint32
        pcNext : uint32
        }

    /// Gets value of specified Register
    let getReg (state:MachineState) (reg: Register) =
        Map.find reg state.RegMap
    
    /// Gets value of High Register
    let getHi (state: MachineState) =
        state.Hi
       
    /// Gets value of Low Register
    let getLo (state: MachineState) =
        state.Lo

    /// Gets value of specified Memory location
    let getMem (state:MachineState) (mem: Memory) =
        Map.find mem state.MemMap

    
    /// Gets current PC value
    let getPC (state:MachineState) =
        state.pc

    /// Gets next PC value
    let getNextPC (state:MachineState) =
        state.pcNext

    /// Sets value into specified Register
    let setReg (state:MachineState) (reg: Register) (data: uint32) =
        let newRegMap = Map.add reg data state.RegMap
        let newState = {state with RegMap = newRegMap}
        newState

    /// Sets value into High Register
    let setHi (state: MachineState) (data: uint32) =
        let newState = {state with Hi = data}
        newState

    /// Sets value into Low Register
    let setLo (state: MachineState) (data: uint32) =
        let newState = {state with Lo = data}
        newState
    
    /// Sets value into specified Memory location
    let setMem (state:MachineState) (mem: Memory) (data: uint32) =
        let newMemMap = Map.add mem data state.MemMap
        let newState = {state with MemMap = newMemMap}
        newState

    /// Prints entire Machine State
    let printState (state:MachineState) =
        printfn "Current Machine State:"
        printfn "PC: \t\t%A" state.pc
        printfn "PCNext: \t%A" state.pcNext
        printfn "State: \t\t%A" state.State

        printfn "Registers:"
        for i in 0..31 do
            printfn "\t\tR%A:\t%A" i state.RegMap.[Register(i)]
        printfn "\t\tHigh:\t%A" state.Hi
        printfn "\t\tLow:\t%A" state.Lo

        printfn "Memory: %A" state.MemMap
    
    /// Initialises Machine State at start of program
    let initialise =
        let regMap =
            let reg = [|0..31|]
            reg |> Array.map (fun i -> (Register(i), 0u)) |> Map.ofArray

        let memMap = Map.empty

        {RegMap=regMap; Hi=0u; Lo=0u; MemMap=memMap; State=RunOK; pc=0u; pcNext=4u}
            
(* // Fronm C compiler -> keeps memory of clock cycles or smg, ct remember
  void advance_pc (SWORD offset)
{
    PC  =  nPC;
   nPC  += offset;
} 
*)
