// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System


open AngelCodeTextRenderer

open GraphicsManagerGLFW
open InputManagerWinRawInput
open SceneGraph2D
open TwoDEngine3
open TwoDEngine3.AsteroidsLevel
open TwoDEngine3.ExampleLevel
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open TwoDEngine3.ManagerInterfaces.InputManager



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

// test func

[<EntryPoint>]
[<STAThread>]
let main argv =

    //Register GraphicsManager
    typedefof<GraphicsManagerGLFW>
    |> ManagerRegistry.addManager
    //register textRenderer
    typedefof<AngelCodeTextRenderer>
    |> ManagerRegistry.addManager
     //register InputManager
    typedefof<InputManagerWinRawInput>
    |> ManagerRegistry.addManager
    

    match ManagerRegistry.getManager<GraphicsManager> () with
    | Some graphics -> graphics.Start(fun gmgr -> SetLevelManager(Some(AsteroidsLevel() :> AbstractLevelController)))

    | None -> printfn "No Graphics Manager registered, check your project references"

    0 // return an integer exit code
