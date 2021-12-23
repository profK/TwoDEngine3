module TDE3ManagerInterfaces.SceneGraphInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

type SceneGraphObjectContainerInterface =
    abstract Children : SceneGraphObjectInterface list
    abstract AddChild: SceneGraphObjectInterface -> SceneGraphObjectContainerInterface
   
    abstract RemoveChild: SceneGraphObjectInterface -> SceneGraphObjectContainerInterface

    abstract FindChild : string list-> SceneGraphObjectInterface option


and SceneGraphObjectInterface =
    inherit SceneGraphObjectContainerInterface
    abstract Name:string
    abstract Path:string
    abstract Parent:SceneGraphObjectContainerInterface 
    
and SceneGraphInterface =
    inherit SceneGraphObjectContainerInterface
   
    
    
    
    
    
      