namespace SceneGraph2D


open System.Reflection.Metadata
open TDE3ManagerInterfaces.SceneGraphInterface

type SceneGraphObjectContainer() =
    let mutable childList = List.Empty
    interface SceneGraphObjectContainerInterface with
        member val Children = childList
        member this.AddChild child : SceneGraphObjectContainerInterface =
            childList <- child::childList
            this
        member this.RemoveChild child : SceneGraphObjectContainerInterface =
            childList <- (childList |> List.except child)
            this
        member this.FindChild path : SceneGraphObjectInterface option =
            childList
            |> List.tryFind (fun child -> child.Name = path.Head)
            |> function
                | Some child ->
                    if path.Tail.Length = 0 then
                        Some child
                    else
                        child.FindChild path.Tail
                | None -> None 
    
