
module TestBench = 
    
    open Expecto

    open Model.Types

    /// the correct answer
    let expected = Model.Model.theAnswer

    /// the current answer to test
    let actual = Answer.theAnswer

    //
    // INSERT TYPES here to randomise eg :  type Days = | D1  | D2 | D3 | D4
    // INSERT RECORDS here eg :  let cTime (d,m,y) :Time = { day = cDay d; month=m ; year=cYear y}
    //

    /// configuration for FsCheck
    let fsConfig = { FsCheck.Config.Default with MaxTest = 500 ; QuietOnSuccess=false} // adjust number of tests according to wishes
   
    /// configuration for Expecto
    let eConfig = { Expecto.Tests.defaultConfig with verbosity = Logging.LogLevel.Info ;  parallel = false; printer = Impl.TestPrinters.defaultPrinter}
    // test function with three parameters
    /// f = function to test
    /// p1c,p2c,p3c = conversion functions for parameters randomised by FsCheck                           

    let testProperty3 pName f p1c p2c p3c =
        testPropertyWithConfig fsConfig  pName 
        <| fun (p1,p2,p3)-> 
            Expect.equal (f actual () (p1c p1) (p2c p2) (p3c p3)) (f expected () (p1c p1) (p2c p2) (p3c p3)) "Answer is wrong"
   
    let testWithSizedArray<'T> n =
        let fixedArrayGen = FsCheck.GenExtensions.ArrayOf(FsCheck.Arb.generate<'T> , n)
        FsCheck.Prop.forAll (FsCheck.Arb.fromGen fixedArrayGen)
    
    /// n - lenth of array of random data
    /// arrayFun - function to test    
    let testSizedArrayProperty n pName arrayFun =
        testPropertyWithConfig {fsConfig with MaxTest=1} pName 
        <| testWithSizedArray n arrayFun

    let properties = 
        testList "FsCheck" 
            [   
                testProperty1 "Q1a answer" (fun r -> r.Q1amInt) id
            ]
    
    [<EntryPoint>]
    let main argv = 
        let retCode = runTests eConfig properties
        System.Console.ReadKey() |> ignore // wait for key to display test results
        retCode // return test result (0 if all passed) to OS for neatness


// 1. install
// 2. try testing with manual (inputs,expectde) pairs through your program first
// 