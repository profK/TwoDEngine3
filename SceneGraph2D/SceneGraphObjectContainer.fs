namespace SceneGraph


open System.Reflection.Metadata
open TDE3ManagerInterfaces.SceneGraphInterface

type SceneGraphObjectContainer() =
    let mutable childList = List.Empty
    interface SceneGraphObjectContainerInterface with
        member this.AddObject(sceneObj:SceneGraphObjectInterface) = 
            childList <- sceneObj::childList
            this
            
        member this.RemoveObject(sceneObj: SceneGraphObjectInterface):
            SceneGraphObjectContainerInterface =
            childList <- childList |> List.except [sceneObj]
            this
            
        member this.RemoveByPath(path: string): SceneGraphObjectContainerInterface =
            (this:>SceneGraphObjectContainerInterface).RemoveByPath(path.Split "." |> Seq.toList)
            this
        member this.RemoveByPath(path: string list): SceneGraphObjectContainerInterface =    
            match path with
            | [_] ->
                childList
                |> List.tryFind (fun node -> node.Name = path.Head)
                |> function
                    | Some node ->
                        (this:> SceneGraphObjectContainerInterface).RemoveObject(node)
                    | None ->
                        this
            | head::tail ->
                childList
                |> List.tryFind (fun node -> node.Name = head)
                |> function
                    |Some node ->
                        (node:> SceneGraphObjectContainerInterface).RemoveByPath(tail)
                        this
                    | None ->
                        this
                        
        member val Children = childList
    
