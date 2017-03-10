module MachineState =
    type Register = R of int
    type Memory = M of int  
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

    let getReg (state:MachineState) (reg: Register) =
        Map.find reg state.RegMap

    let getHi (state: MachineState) =
        state.Hi

    let getLo (state: MachineState) =
        state.Lo

    let setReg (state:MachineState) (reg: Register) (data: uint32) =
        let newRegMap = Map.add reg data state.RegMap
        let newState = {state with RegMap = newRegMap}
        newState

    let setHi (state: MachineState) (data: uint32) =
        let newState = {state with Hi = data}
        newState

    let setLo (state: MachineState) (data: uint32) =
        let newState = {state with Lo = data}
        newState

    let getMem (state:MachineState) (mem: Memory) =
        Map.find mem state.MemMap

    let setMem (state:MachineState) (mem: Memory) (data: uint32) =
        let newMemMap = Map.add mem data state.MemMap
        let newState = {state with MemMap = newMemMap}
        newState



            
(* // Fronm C compiler -> keeps memory of clock cycles or smg, ct remember
  void advance_pc (SWORD offset)
{
    PC  =  nPC;
   nPC  += offset;
} 
*)
