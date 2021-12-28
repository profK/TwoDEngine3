namespace SceneGraph2D
open FSTree

open FSTree.Tree
open TDE3ManagerInterfaces.SceneGraphInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

       
type SceneGraphNode = TreeNode<SceneGraphInterface> 
type SG2DSceneGraph() as this =
    let mutable tree= Tree.Empty
    do ManagerRegistry.getManager<GraphicsManager>()
       |> function
           | Some gm ->
               gm.GraphicsListeners <- this::gm.GraphicsListeners
           | None ->
               printfn("Error: SG2DSceneGraph failed to find graphics manager")
   
    interface SceneGraphInterface with
        member this.AddChild (child) : SceneGraphInterface=
            tree.AddChild (TreeNode (Some child)) |> ignore
            this
        member this.RemoveChild (child) : SceneGraphInterface =
            tree.FindChild (Some child)
            |> function
                | Some node ->
                    tree.RemoveChild node |> ignore
                | None -> ()
            this

    
        
    interface GraphicsListener with
        member this.Render(graphics) =
            Tree.iter (fun parent child  ->
                    match child.Data with
                    | Some (data:SceneGraphObjectInterface) -> data.Render graphics
                    | None -> ()
                ) tree
        member this.Update graphics deltaT =
            tree <-
              tree
              |> Tree.fold (fun state parentNode childNode  ->
                     let newData=
                        (match childNode.Data with
                            | Some data ->
                                data.Update graphics deltaT 
                            | None ->
                                None
                        )
                     state.Children <- TreeNode(newData)::state.Children
                     state
                ) Tree.Empty 
            None // no error interrupting right now, probably needs to be a fold