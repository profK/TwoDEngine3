// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp


open GraphicsMgrMonoGameDesktop
open TwoDEngine3.ExampleLevel
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface


let mutable currentLevelController: AbstractLevelController option = None

let SetLevelManager newLevelManger : unit =
    match currentLevelController with
    | Some oldLevelMgr ->
        oldLevelMgr.Close()
        ()
    | None -> ()

    currentLevelController <- newLevelManger

    match currentLevelController with
    | Some mgr -> mgr.Open()
    | None -> ()

let Update deltaMS =
    printfn $"Update deltams=%d{deltaMS}"
    None // no errors or other reason to quit

let Render unit =
    printfn "Render"
    ()


[<EntryPoint>]
let main argv =

    //Register GraphicsManager
    typedefof<GraphicsManagerMGDT>
    |> ManagerRegistry.addManager

    // create lvel managers and set the active one hereSome(BouncyBall:>AbstractLevelController)

    let lm =
        Some(BouncyBall() :> AbstractLevelController)

    SetLevelManager lm

    match ManagerRegistry.getManager<GraphicsManager> () with
    | Some graphics -> graphics.Start()
    | None -> printfn "No Graphics Manager registered, check your project references"

    0 // return an integer exit code
