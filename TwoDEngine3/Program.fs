// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open AngelCodeTextRenderer
open GraphicsMgrMonoGameDesktop
open ManagerRegistry
open TwoDEngine3.ExampleLevel
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open TwoDEngine3.ManagerInterfaces.InputManager
open USBInputManager


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
[<STAThread>]
let main argv =

    //Register GraphicsManager
    typedefof<GraphicsManagerMGDT>
    |> ManagerRegistry.addManager
    //register textRenderer
    typedefof<AngelCodeTextRenderer>
    |> ManagerRegistry.addManager
    //register input manager
    typedefof<USBInputManager>
    |> ManagerRegistry.addManager

    // create lvel managers and set the active one hereSome(BouncyBall:>AbstractLevelController)

    //test of usb scan
    (ManagerRegistry.getManager<InputManager>().Value :?> USBInputManager).AxisTest
    |> Seq.iter (fun s ->
            Console.WriteLine s
        )

    match ManagerRegistry.getManager<GraphicsManager> () with
    | Some graphics -> graphics.Start(fun gmgr -> SetLevelManager(Some(BouncyBall() :> AbstractLevelController)))

    | None -> printfn "No Graphics Manager registered, check your project references"

    0 // return an integer exit code
