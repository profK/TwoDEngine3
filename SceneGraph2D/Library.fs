namespace SceneGraph2D
open FSTree


open ManagerRegistry
open TwoDEngine3.ManagerInterfaces
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

type SceneGraphObject =
    abstract Update: uint->string option
    abstract Render: GraphicsManager->unit
    
type SpriteObject(image, transform) =
    member val Transform = transform
    member val Image = image
    interface SceneGraphObject with
        override this.Render(graphics) =
            graphics.PushMultTransform this.Transform
            graphics.DrawImage this.Image
            graphics.PopTransform() |> ignore
        override this.Update(deltaTime) =
            None // no action, no error
       

type SG2DSceneGraph() as this =
    let tree= Tree.Empty
    do ManagerRegistry.getManager<GraphicsManager>()
       |> function
           | Some gm ->
               gm.GraphicsListeners <- this::gm.GraphicsListeners
           | None ->
               printfn("Error: SG2DSceneGraph failed to find graphics manager")
   
    member this.AddChild (child) : SG2DSceneGraph=
        tree.AddChild child |> ignore
        this
    member this.RemoveChild (child) : SG2DSceneGraph =
        tree.RemoveChild child |> ignore
        this
        
    interface GraphicsListener with
        member this.Render(graphics) =
            Tree.iter (fun parent child level ->
                    match child.Data with
                    | Some (data:SceneGraphObject) -> data.Render graphics
                    | None -> ()
                ) tree
        member this.Update(deltaT) =
            Tree.iter (fun parent child level ->
                    match child.Data with
                    | Some (data:SceneGraphObject) -> data.Update deltaT |> ignore
                    | None -> ()
                ) tree
            None // no error interrupting right now, probably needs to be a fold