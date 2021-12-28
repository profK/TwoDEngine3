module TwoDEngine3.LevelManagerInterface

open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

[<AbstractClass>]
type AbstractLevelController() =
    member val paused = false with get, set
    member val graphics = ManagerRegistry.getManager<GraphicsManager> ()


    interface GraphicsListener with
        member this.Update graphics deltaMS : string option =
            this.UpdateImpl graphics deltaMS

        member this.Render graphics : unit = this.RenderImpl graphics

    abstract UpdateImpl : GraphicsManager->uint -> string option

    default this.UpdateImpl graphics deltaMS : string option =
        printfn "deltaMS = %d{deltaMS}" |> ignore
        None

    abstract RenderImpl : GraphicsManager -> unit

    default this.RenderImpl graphicsManager : unit =
        printfn "Render" |> ignore
        ()

    abstract member Open : unit -> unit

    default this.Open() =
        this.graphics.Value.GraphicsListeners <-
            (this :> GraphicsListener)
            :: this.graphics.Value.GraphicsListeners

    abstract member Close : unit -> unit

    default this.Close() =
        this.graphics.Value.GraphicsListeners <-
            this.graphics.Value.GraphicsListeners
            |> List.except (seq { this :> GraphicsListener })

    abstract member Continue : unit -> unit
    default this.Continue() = this.paused <- false

    abstract member Pause : unit -> unit
    default this.Pause() = this.paused <- true
