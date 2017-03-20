namespace VisualMIPS

module Jtypes = 
    open Types
    open Instructions
    open MachineState


    //fullJ functions

    let opJ (mach: MachineState) (instr : Instruction) (Targetval target) =
        // 4026531840 is 11110..0 //// 268435455 is 00001..1
        let address =  (T.getValue(getNextPC mach) &&& 4026531840u ) + ( (target <<< 2) &&& 268435455u ) 
        setNextNextPC (Word(address)) mach
       
    
    let opJAL (mach: MachineState) (Targetval target) =
        failwith "not implemented yet"
        //let address =  (T.getValue(getNextPC mach) &&& 4026531840u ) + ( (target <<< 2) &&& 268435455u ) 
        //setNextNextPC (Word(address)) mach
        // loads in rA the return address which is address of jal + 8 //rA is reg31
        // load PC with subroutine entry point (located in target) 
        // so that subr exe and then nextnextPC takes whats in 31 and executes it
        // a branch delay slot follows up (but are we doing them ?)


// to return from a subroutine, JR is used : jr $ra means go to return address in $ra (ends a subroutine)
// if a subr calls a subr then need STACK to preserve return address ra... -> meaning MERGE actual map and call 
// back map from a subr