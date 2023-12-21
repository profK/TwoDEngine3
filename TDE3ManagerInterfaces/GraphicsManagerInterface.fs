module TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

open System.IO
open System.Numerics

type Vector = System.Numerics.Vector2
let Vector(x,y) = Vector2(x,y)
   
     
type Rectangle(pos, sz) =
    member val Position:Vector = pos
    member val Size:Vector =sz with get
type Image =

    abstract SubImage : Rectangle -> Image
    abstract Size : Vector with get

type Transform =
    abstract Multiply : Vector -> Vector
    abstract Multiply : Transform -> Transform
    

type GraphicsListener =
    abstract Update : GraphicsManager->uint -> string option
    abstract Render : GraphicsManager -> unit

and GraphicsManager =
    abstract GraphicsListeners : GraphicsListener list with get, set
    abstract ScreenSize : Vector
    abstract LoadImage : Stream -> Image
    abstract PushClip : Rectangle -> unit
    abstract PopClip : unit -> Rectangle option
    abstract PushTransform : Transform -> unit
    abstract PopTransform : unit -> Transform option
    abstract PeekTransform : unit -> Transform option
    abstract PushMultTransform: Transform -> unit
    abstract DrawImage : Image -> unit
    abstract Start : (GraphicsManager -> unit) -> unit
    abstract Start : unit -> unit
    abstract IdentityTransform : Transform with get
    abstract RotationTransform : float32 -> Transform
    abstract TranslationTransform : float32-> float32 -> Transform
    
    abstract ScaleTransform : float32-> float32 -> Transform

    
