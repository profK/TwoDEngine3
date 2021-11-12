module TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

open System.IO
open System.Numerics

[<AbstractClass>]
type Vector() =
    abstract X:float32 with get
    abstract Y:float32 with get
    
    abstract Plus: Vector -> Vector
    static member (+) (v : Vector, a: Vector) =
        v.Plus(a)
     
type Rectangle(pos, sz) =
    member val Position:Vector = pos
    member val Size:Vector =sz
type Image =

    abstract SubImage : Rectangle -> Image
    abstract Size : Vector

type Transform =
    abstract Multiply : Vector -> Vector
    abstract Multiply : Transform -> Transform

type GraphicsListener =
    abstract Update : int -> string option
    abstract Render : GraphicsManager -> unit

and GraphicsManager =
    abstract GraphicsListeners : GraphicsListener list with get, set
    abstract ScreenSize : Vector
    abstract LoadImage : Stream -> Image
    abstract PushClip : Rectangle -> unit
    abstract PopClip : unit -> Rectangle option
    abstract PushTransform : Transform -> unit
    abstract PopTransform : unit -> Transform option
    abstract DrawImage : Image -> Vector -> unit
    abstract Start : (GraphicsManager -> unit) -> unit
    abstract Start : unit -> unit
    abstract IdentityTransform : Transform with get
    abstract RotationTransform : float32 -> Transform
    abstract TranslationTransform : float32-> float32 -> Transform
    abstract NewVector: float32 ->float32->Vector
    
