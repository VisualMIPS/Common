namespace VisualMIPS

module Itypes = 
    open Types
    open Instructions
    open MachineState


    //fullI functions

    let opADDI (mach: MachineState) (instr : Instruction) (Word rS) (Half immediate) =
        let immediateSigned = int16 immediate
        let output32 =  int64( int32(rS) + int32(immediateSigned) )
        let output64 =  int64( int32( rS)) + int64(immediateSigned)
        match (output32=output64) with
        | true -> 
            let output = Word( rS + uint32(immediateSigned) )
            (output, mach)
        | false -> //overflow occured
            let outputSameRd = getReg instr.rd mach
            let newMach = setState (RunTimeErr "Overflow on ADDI") mach
            (outputSameRd , newMach)

    //memI functions

    let opLB (mach: MachineState) (instr : Instruction) (rT) (Word myBase) (Half offset) =
        //rT unused //load byte from memory as a signed value
        let offsetSigned = uint32( int16 offset) //offset always read as signed
        let address = Memory( myBase + offsetSigned )
        let outputByte = T.getValue( getMem address mach )
        let outputWord = Word( uint32( sbyte ( outputByte ))) //sign extends it
        let newMach = setReg instr.rt outputWord mach
        newMach
    
    let opLBU (mach: MachineState) (instr : Instruction) (rT) (Word myBase) (Half offset) =
        //rT unused
        let offsetSigned = uint32( int16 offset)
        let address = Memory( myBase + offsetSigned )
        let outputByte = T.getValue( getMem address mach )
        let outputWord = Word( uint32( outputByte )) //pads with zeros
        let newMach = setReg instr.rt outputWord mach
        newMach
    
    let opLH (mach: MachineState) (instr : Instruction) (rT) (Word myBase) (Half offset) =
         let offsetSigned = uint32( int16 offset)
         let addressL = Memory( myBase + offsetSigned )
         let returnMach =
            match ((T.getValue( addressL ) &&& 1u) = 0u) with //check LSB is 0 (=mult of 2)
            | true -> 
                let outputHalfL = uint16( T.getValue( getMem addressL mach ))
                let addressM = Memory( myBase + offsetSigned + 1u )
                let outputHalfM = uint16( T.getValue( getMem addressM mach ))
                let outputHalf = (outputHalfM <<< 8 ) + outputHalfL
                let outputWord = Word( uint32( int16 outputHalf )) //sign extends it
                let newMachT = setReg instr.rt outputWord mach
                newMachT
            | false -> //raise exception if effective address not naturally aligned
                let newMachF = setState (RunTimeErr "Address Error on LH") mach
                newMachF
         returnMach
      
    
    let opLHU (mach: MachineState) (instr : Instruction) (rT) (Word myBase) (Half offset) =
         let offsetSigned = uint32( int16 offset)
         let addressL = Memory( myBase + offsetSigned )
         let returnMach =
            match ((T.getValue( addressL ) &&& 1u) = 0u) with //check LSB is 0 (=mult of 2)
            | true -> 
                let outputHalfL = uint16( T.getValue( getMem addressL mach ))
                let addressM = Memory( myBase + offsetSigned + 1u )
                let outputHalfM = uint16( T.getValue( getMem addressM mach ))
                let outputHalf = (outputHalfM <<< 8 ) + outputHalfL
                let outputWord = Word( uint32( outputHalf )) // zero extended
                let newMachT = setReg instr.rt outputWord mach
                newMachT
            | false -> //raise exception if effective address not naturally aligned
                let newMachF = setState (RunTimeErr "Address Error on LHU") mach
                newMachF
         returnMach
    
    let opLW (mach: MachineState) (instr : Instruction) (rT) (Word myBase) (Half offset) =
         let offsetSigned = uint32( int16 offset)
         let returnMach =
            match (((myBase + offsetSigned) &&& 3u) = 0u) with //check last 2 B are 0 (=mult of 4)
            | true -> 
                //manually
                (*
                let output1 = uint32( T.getValue( getMem address1 mach ))
                let address2 = Memory( myBase + offsetSigned + 1u )
                let output2 = uint32( T.getValue( getMem address2 mach ))
                let address3 = Memory( myBase + offsetSigned + 2u )
                let output3 = uint32( T.getValue( getMem address3 mach ))
                let address4 = Memory( myBase + offsetSigned + 3u )
                let output4 = uint32( T.getValue( getMem address4 mach ))
                let output = (output4 <<< 24 ) + (output3 <<< 16 ) + (output2 <<< 8 ) + output1
                *)
                //declaratively
                let output =
                    let createAddress x = Memory( myBase + offsetSigned + x ) 
                    let loadContent x = uint32( T.getValue( getMem x mach ))
                    let createWord x = List.fold (fun acc y -> (acc <<< 8) + y) 0u x
                    [3u; 2u; 1u; 0u] //[0u..3u]
                    |> List.map (createAddress >> loadContent) 
                    |> createWord
                let outputWord = Word( output ) 
                let newMachT = setReg instr.rt outputWord mach
                newMachT
            | false -> //raise exception if effective address not naturally aligned
                let newMachF = setState (RunTimeErr "Address Error on LW") mach
                newMachF
         returnMach

    let opLWL (mach: MachineState) (instr : Instruction) (Word rT) (Word myBase) (Half offset) =
        let offsetSigned = uint32( int16 offset)
        let byteNumber = ((myBase + offsetSigned) &&& 3u) //0,1,2 or 3
        let fetchedBytes =
            let createAddress x = Memory( myBase + offsetSigned + x )
            let loadContent x = uint32( T.getValue( getMem x mach ))
            let createBytes x = (List.fold (fun acc y -> (acc <<< 8) + y ) 0u x) <<< 8*int32(byteNumber)
            [byteNumber..3u]
            |> List.map (createAddress >> loadContent)
            |> createBytes
        let modifRt =  
            let shiftToEraseVal = 8*int32( 4u - byteNumber )
            match (byteNumber=0u) with //fixes wrap around of the <<</>>> (Undocumented by F#)
            | false -> Word( ( ( rT <<< shiftToEraseVal ) >>> shiftToEraseVal ) + fetchedBytes )
            | true -> Word(fetchedBytes)
        let returnMach = setReg instr.rt modifRt mach
        returnMach
    let opLWR (mach: MachineState) (instr : Instruction) (Word rT) (Word myBase) (Half offset) =
        let offsetSigned = uint32( int16 offset)
        let byteNumber = ((myBase + offsetSigned) &&& 3u) //0,1,2 or 3
        let fetchedBytes =
            let createAddress x = Memory( myBase + offsetSigned + x )
            let loadContent x = uint32( T.getValue( getMem x mach ))
            let createBytes x = List.fold (fun acc y -> (acc <<< 8) + y ) 0u x
            [0u..byteNumber]
            |> List.map (createAddress >> loadContent)
            |> createBytes
        let modifRt =  
            let shiftToEraseVal = 8*int32( byteNumber + 1u )
            Word( ( ( rT >>> shiftToEraseVal ) <<< shiftToEraseVal ) + fetchedBytes )
        let returnMach = setReg instr.rt modifRt mach
        returnMach

    let opSB (mach: MachineState) (instr : Instruction) (Word rT) (Word myBase) (Half offset) =
         let offsetSigned = uint32( int16 offset)
         let address = Memory( myBase + offsetSigned )
         let rTByte = Byte( byte rT ) //select the 8 LSB of rT
         let returnMach =
            setMem address rTByte mach
         returnMach
    
    let opSH (mach: MachineState) (instr : Instruction) (Word rT) (Word myBase) (Half offset) =
         let offsetSigned = uint32( int16 offset)
         let addressL = Memory( myBase + offsetSigned )
         let returnMach =
            match ((T.getValue( addressL ) &&& 1u) = 0u) with //check LSB is 0 (=mult of 2)
            | true -> 
                let rTByteL = Byte( byte rT ) //select bits 7..0 of rT
                let rTByteM = Byte( byte (rT >>> 8 )) //select bits 15..8 of rT
                let addressM = Memory( myBase + offsetSigned + 1u )
                let newMachT =
                    mach
                    |> setMem addressL rTByteL
                    |> setMem addressM rTByteM
                newMachT
            | false -> //raise exception if effective address not naturally aligned
                let newMachF = setState (RunTimeErr "Address Error on SH") mach
                newMachF
         returnMach

    let opSW (mach: MachineState) (instr : Instruction) (Word rT) (Word myBase) (Half offset) =
            let offsetSigned = uint32( int16 offset)
            let returnMach =
                match (((myBase + offsetSigned) &&& 3u) = 0u) with //check last 2 B are 0 (=mult of 4)
                | true ->
                    let newMachT =
                        let createAddress x = Memory( myBase + offsetSigned + x )
                        let createByte x = Byte( byte ( rT >>> ( 8 * int32( x ))))
                        let storeWord x = List.fold (fun acc (add,byt) -> setMem add byt acc) mach x
                        [0u; 1u; 2u; 3u]
                        |> List.map (fun x -> ( createAddress x , createByte x ) )
                        |> storeWord 
                    newMachT
                | false -> //raise exception if effective address not naturally aligned
                    let newMachF = setState (RunTimeErr "Address Error on SW") mach
                    newMachF
            returnMach
    
    
    
