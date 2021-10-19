module TwoDEngine3.LevelManagerInterface

open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

[<AbstractClass>]
type AbstractLevelController() =
    let graphics = ManagerRegistry.getManager<GraphicsManager>()
    let mutable paused = false
    interface GraphicsListener with
        member this.Update deltaMS : string option =
            this.UpdateImpl deltaMS
        
        member this.Render graphics : unit =
            this.RenderImpl graphics
             
    abstract UpdateImpl: int->string option
    default this.UpdateImpl deltaMS : string option =
            printfn "deltaMS = %d{deltaMS}" |> ignore
            None
            
    abstract RenderImpl: GraphicsManager -> unit
    default this.RenderImpl graphicsManager : unit =
            printfn "Render" |> ignore
            ()
            
    abstract member Open: unit->unit
    default this.Open() = 
        graphics.Value.GraphicsListeners <-
            (this:>GraphicsListener) :: graphics.Value.GraphicsListeners
    
    abstract member Close: unit -> unit
    default this.Close() =
        graphics.Value.GraphicsListeners <-
            graphics.Value.GraphicsListeners
            |> List.except(seq{this:>GraphicsListener})
            
    abstract member Continue: unit->unit
    default this.Continue() =
        paused <- false
    
    abstract member Pause: unit->unit
    default this.Pause() = 
        paused <- true
