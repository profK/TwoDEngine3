module TDE3ManagerInterfaces.SceneGraphInterface

open TDE3ManagerInterfaces
type SceneGraphObjectContainerInterface =
    abstract AddObject: SceneGraphObjectInterface -> SceneGraphObjectContainerInterface
    abstract RemoveByPath: string -> SceneGraphObjectContainerInterface
    abstract RemoveByPath: string list -> SceneGraphObjectContainerInterface
    abstract RemoveObject: SceneGraphObjectInterface -> SceneGraphObjectContainerInterface
    abstract Children : SceneGraphObjectInterface list


and SceneGraphObjectInterface =
    inherit SceneGraphObjectContainerInterface
    abstract Name:string
    abstract Path:string
    abstract Parent:SceneGraphObjectContainerInterface 
    
and SceneGraphInterface =
    inherit SceneGraphObjectContainerInterface
    abstract Update: uint -> string option
    abstract Render: unit -> unit
    
    
    
    
      