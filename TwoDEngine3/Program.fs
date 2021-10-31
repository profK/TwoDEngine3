// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System

open System.IO
open AngelCodeTextRenderer
open GraphicsMgrMonoGameDesktop

open TwoDEngine3.ExampleLevel
open TwoDEngine3.LevelManagerInterface
open HIDInputManager
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
let rec PrintControl (node: Node, writer: TextWriter, indent: int) =
    let spaces = new string (' ', indent)
    fprintfn writer $"%s{spaces + (node.Name)}"

    match node.Value with
    | Children children ->
        children
        |> Seq.iter (fun child -> PrintControl(child, writer, indent + 4))
    | Axis axis ->
        let spaces = new string (' ', indent + 4)

        match axis with
        | Digital b -> fprintfn writer $"%s{spaces}Digital = %s{b.ToString()}"
        | Analog a -> fprintfn writer $"%s{spaces}Analog = %s{a.ToString()}"
        | Keyboard k -> fprintfn writer $"%s{spaces}Keyboard = %s{k.ToString()}"

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
    typedefof<HIDInputManager>
    |> ManagerRegistry.addManager

    // create lvel managers and set the active one hereSome(BouncyBall:>AbstractLevelController)
    let inputMgr =
        ManagerRegistry.getManager<InputManager>().Value

    PrintControl(inputMgr.controllerRoot, Console.Out, 0)

    match ManagerRegistry.getManager<GraphicsManager> () with
    | Some graphics -> graphics.Start(fun gmgr -> SetLevelManager(Some(BouncyBall() :> AbstractLevelController)))

    | None -> printfn "No Graphics Manager registered, check your project references"

    0 // return an integer exit code
