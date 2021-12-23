namespace SceneGraph2D

open SceneGraph2D
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

[<AbstractClass>]
type SG2DPositionedObject<'T when 'T :> SG2DPositionedObject<'T> >
    (parent:SceneGraphObjectContainer, name:string, xform:Transform)=
    inherit SceneGraph2DObject(name,parent)
   
    member val Xform=xform  with get
    
    abstract RenderImpl: GraphicsManager -> unit
    default this.RenderImpl graphicsManager = ()
    override this.Render graphicsManager =
        graphicsManager.PushMultTransform this.Xform
        this.RenderImpl graphicsManager
        base.Render graphicsManager // IMPORTAnT, passes render down tree
        graphicsManager.PopTransform() |> ignore
    
    abstract Transform: Transform -> 'T
   
        
type SG2DImageObject(parent:SceneGraphObjectContainer, name:string, xform:Transform,
        image:Image) =
    inherit SG2DPositionedObject<SG2DImageObject>(parent, name, xform)
    
    member val Image=image with get
    override this.Transform transform :  SG2DImageObject =
        SG2DImageObject(parent,name,
            this.Xform.Multiply transform, this.Image )
    override this.RenderImpl graphicsManager =
        graphicsManager.DrawImage this.Image