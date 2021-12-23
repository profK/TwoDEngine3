module TDE3ManagerInterfaces.SceneGraphInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

type UpdateFunction = SceneGraphObjectContainerInterface ->
                        SceneGraphObjectContainerInterface list ->
                        SceneGraphObjectContainerInterface    
and SceneGraphObjectContainerInterface =
    abstract Children : SceneGraphObjectInterface list
    abstract AddChild: SceneGraphObjectInterface -> SceneGraphObjectContainerInterface
   
    abstract RemoveChild: SceneGraphObjectInterface -> SceneGraphObjectContainerInterface

    abstract FindChild : string list-> SceneGraphObjectInterface option

    abstract Update: UpdateFunction  ->
                       SceneGraphObjectContainerInterface
    

and SceneGraphObjectInterface =
    inherit SceneGraphObjectContainerInterface
    abstract Name:string
    abstract Path:string
    abstract Parent:SceneGraphObjectContainerInterface 
    
and SceneGraph2DInterface =
    inherit SceneGraphObjectContainerInterface
    abstract MakeSprite: SceneGraphObjectContainerInterface -> string ->
        Transform -> Image -> SpriteInterface 
    
and SpriteInterface =
    inherit SceneGraphObjectInterface
    abstract Transform:
        Transform->SceneGraphObjectContainerInterface->SpriteInterface
    abstract Image: Image
    
   
  
    
    
    
    
      