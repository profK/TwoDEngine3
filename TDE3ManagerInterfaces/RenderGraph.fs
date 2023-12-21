module TwoDEngine3.RenderGraphInterface

open System
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface


type RenderNode([<ParamArray>] childrenArray:RenderNode array ) =
    let Children = childrenArray
    
    abstract member Render: Transform-> GraphicsManager -> unit
    default this.Render xform gm =
        this.RenderChildren xform gm
    member this.RenderChildren xform gm =    
        Children
        |> Array.iter (fun child -> child.Render xform gm)

    
type Sprite(img:Image, [<ParamArray>] childrenArray:RenderNode array) =
    inherit RenderNode(childrenArray)
    let Image = img
    override this.Render xform gm =
        gm.DrawImage img
        this.RenderChildren xform gm
       
type GenericTransform(transform:Transform, [<ParamArray>] childrenArray:RenderNode array) =
    inherit RenderNode(childrenArray)
    let myXform = transform
    override this.Render xform gm =
        let newXform = xform.Multiply myXform
        this.RenderChildren newXform gm        
        
type Rotate(degrees:float, [<ParamArray>] childrenArray:RenderNode array) =
    inherit GenericTransform(gm.Rot,  childrenArray)    
    
        
    

