module TDE3ManagerInterfaces.SceneGraphInterface

open System
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
(***************************************************************
This file is special in that it does not act as a front end to 
pluggable back end code.  Rather it defines a scenegraph DSl used
directly by the game application.
It is in the Interfaces namespace for convenience
****************************************************************)
type SceneGraphObject([<ParamArray>] childlist: SceneGraphObject array) =
    let initialChildList = childlist
    abstract Update: GraphicsManager->uint->SceneGraphObject option
    default this.Update gm deltaMs = Some this
    abstract Render: GraphicsManager->unit
    default this.Render gm = ()
    member val Xform:Transform = Transform.Identity with get, set
    member val Children=initialChildList with get,set
    
type Sprite( img:Image,  [<ParamArray>] childlist: SceneGraphObject array) = 
    inherit SceneGraphObject(childlist)
    let image = img
    
    override this.Render gm  =
        match gm.PopTransform() with
        | Some pt -> 
                gm.PushTransform (base.Xform.Multiply pt)
        | None -> gm.PushTransform (base.Xform)
        gm.DrawImage image
        Array.iter (
            fun (child:SceneGraphObject) ->
                child.Render gm
            ) base.Children
        gm.PopTransform() |> ignore
        ()
    
    
        
 
    
   
  
    
    
    
    
      