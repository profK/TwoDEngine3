module TDE3ManagerInterfaces.SceneGraphInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

   
type SceneGraphObjectInterface =
    abstract Name:string
    abstract Children : SceneGraphObjectInterface list
    abstract AddChild: SceneGraphObjectInterface -> SceneGraphObjectInterface
   
    abstract RemoveChild: SceneGraphObjectInterface -> SceneGraphObjectInterface
    
    abstract RemoveAll: unit -> SceneGraphObjectInterface

    abstract FindChild : string list-> SceneGraphObjectInterface option

    abstract Update: uint -> SceneGraphObjectInterface 
    abstract Render: GraphicsManager -> unit
    
    
and SceneGraph2DInterface =
    inherit SceneGraphObjectInterface
    abstract MakeSprite: SceneGraphObjectInterface -> string ->
        Transform -> Image -> SpriteInterface
    
and SpriteInterface =
    inherit SceneGraphObjectInterface
    abstract Transform:
        Transform->SceneGraphObjectInterface->SpriteInterface
    abstract Image: Image
    
   
  
    
    
    
    
      