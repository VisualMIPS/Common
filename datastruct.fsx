 module MachineState =
        type Register = R of int
        type Memory = M of int  
        type RunState = 
        | RunOK
        | RunTimeErr of string
        | SyntaxErr of string
        type Flags = 
            {
                N: bool
                Z: bool
                C: bool
                V: bool
            }
        type MachineState = 
            { 
                RegMap : Map<Register, int>
                MemMap : Map<Memory, int> 
                Flags : Flags 
                State : RunState 
            }
            
(* 
  void advance_pc (SWORD offset)
{
    PC  =  nPC;
   nPC  += offset;
} 
*)
