module TwoDEngine3.RenderGraphInterface

open System
open TwoDEngine3.ManagerInterfaces
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface


type RenderNode(childrenArray:RenderNode list,
                graphicsManager:GraphicsManager) =
    let Children = childrenArray
    member val Gm = graphicsManager with get
    
    abstract member Render: Transform-> unit
    default this.Render xform =
        this.RenderChildren xform 
    member this.RenderChildren xform  =    
        Children
        |> List.iter (fun child -> child.Render xform)

 type RootNode(childrenArray:RenderNode list,
                graphicsManager:GraphicsManager) =
    inherit RenderNode(childrenArray,graphicsManager)
    
    
    
type SpriteNode(img:Image,
                childrenArray:RenderNode list,
                graphicsManager:GraphicsManager) =
    inherit RenderNode(childrenArray, graphicsManager)
    let Image = img
    override this.Render xform  =
        base.Gm.DrawImage img
        this.RenderChildren xform 
       
type GenericTransformNode(transform:Transform,
                          childrenArray:RenderNode list,
                          gm:GraphicsManager) =
    inherit RenderNode(childrenArray,gm)
    let myXform = transform
    override this.Render xform  =
        let newXform = xform.Multiply myXform
        this.RenderChildren newXform      
        
type RotateNode(degrees:float32,  childrenArray:RenderNode list,
            gm:GraphicsManager) =
    inherit GenericTransformNode(gm.RotationTransform degrees,
                             childrenArray,gm)    
    
 type TranslateNode(x:float32, y:float32,
                    childrenArray:RenderNode list,
                    gm:GraphicsManager) =
    inherit GenericTransformNode(gm.TranslationTransform x y,
                             childrenArray,gm)       
    
(*********************Builder Functions************************
These functions define the domain sepcific language that can be used 
to build a render tree
****************************************************************)

let private ProcessFuncs gm (funcs:(GraphicsManager->RenderNode) list) =
    funcs
    |> List.map (fun func -> func gm )
    
let  RENDERTREE gm childrenArray =
    ProcessFuncs gm childrenArray
    
let SPRITE img childrenArray graphicsManager =
    let childnodes = ProcessFuncs graphicsManager childrenArray
    new SpriteNode(img, childnodes, graphicsManager) :> RenderNode
   
let ROTATE degrees childrenArray graphicsManager =
     let childnodes = ProcessFuncs graphicsManager childrenArray
     new RotateNode(degrees, childnodes, graphicsManager)
let TRANSLATE x y childrenArray graphicsManager =
     let childnodes = ProcessFuncs graphicsManager childrenArray
     new TranslateNode(x,y, childnodes, graphicsManager)       

