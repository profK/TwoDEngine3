namespace GraphicsManagerVeldrid

open System
open ManagerRegistry
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open Veldrid
open Veldrid.StartupUtilities


[<Manager("Veldrid based graphics manager",supportedSystems.Windows)>]
type GraphicsManagerVeldrid() =
    interface GraphicsManager with
        member val GraphicsListeners  = List.empty with get,set
        member this.DrawImage(var0) = failwith "todo"
        member this.LoadImage(var0) = failwith "todo"
        member this.PopClip() = failwith "todo"
        member this.PopTransform() = failwith "todo"
        member this.PushClip(var0) = failwith "todo"
        member this.PushTransform(var0) = failwith "todo"

        member this.Start() =
            let graphicsManager = (this:>GraphicsManager)
            let IterateUpdateTilError (deltaMS)  =
                    graphicsManager.GraphicsListeners
                    |> List.tryPick (fun listener ->  listener.Update deltaMS)
                    |>  function 
                        | Some(error) ->
                                printfn $"Quitting because %s{error}"
                                false
                        | None -> true
                            
                          
            let windowCI =
                new WindowCreateInfo(
                    X = 100,
                    Y = 100,
                    WindowWidth = 960,
                    WindowHeight = 540,
                    WindowTitle = "Veldrid Tutorial")
            let window = VeldridStartup.CreateWindow(windowCI:WindowCreateInfo )
            
            let options =
                new GraphicsDeviceOptions(
                    PreferStandardClipSpaceYDirection = true,
                    PreferDepthRangeZeroToOne = true )
            
            let graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options)
            let factory = graphicsDevice.ResourceFactory
            let commandList = factory.CreateCommandList()
            
            let mutable lastTime = DateTime.Now.Millisecond
            let mutable thisTime = DateTime.Now.Millisecond
           
            while IterateUpdateTilError (thisTime-lastTime) do
                commandList.Begin()
                commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
                commandList.ClearColorTarget(0u , RgbaFloat.Black);
                graphicsManager.GraphicsListeners
                |> List.iter (fun listener -> listener.Render(this))
                commandList.End();
                graphicsDevice.SubmitCommands(commandList)
                graphicsDevice.SwapBuffers();
                lastTime <-thisTime
                thisTime <- DateTime.Now.Millisecond        
            () 
            
        