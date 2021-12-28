module TDE3ManagerInterfaces.SceneGraphInterface
open TDE3ManagerInterfaces
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

type SceneGraphObjectInterface =
    abstract Update: GraphicsManager->uint->SceneGraphObjectInterface option
    abstract Render: GraphicsManager->unit
   

type SceneGraphInterface =
    abstract  AddChild: SceneGraphObjectInterface -> SceneGraphInterface
    abstract  RemoveChild: SceneGraphObjectInterface -> SceneGraphInterface
    
       
   
 
    
   
  
    
    
    
    
      