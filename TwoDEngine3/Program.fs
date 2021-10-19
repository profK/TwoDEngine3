// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp


open System


open TwoDEngine3.ExampleLevel
open TwoDEngine3.LevelManagerInterface


let mutable currentLevelManager: AbstractLevelController option = None

let SetLevelManager newLevelManger : unit =
    match currentLevelManager with
    | Some oldLevelMgr ->
        oldLevelMgr.Close ()
        ()
    | None -> ()
    currentLevelManager <- newLevelManger
    match currentLevelManager with
    | Some mgr -> mgr.Open ()
    | None -> ()
    
let Update deltaMS =
    printfn $"Update deltams=%d{deltaMS}"
    None // no errors or other reason to quit
    
let Render unit =
    printfn "Render"
    ()

    
[<EntryPoint>]
let main argv =
   
    //load plugins
    AppDomain.CurrentDomain.GetAssemblies()
    |> Array.iter(ManagerRegistry.scanAssembly)
    
    // create lvel managers and set the active one hereSome(BouncyBall:>AbstractLevelController)
    
    let lm = Some(BouncyBall():>AbstractLevelController)
    SetLevelManager lm
    0 // return an integer exit code