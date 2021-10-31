module TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

open System.IO
open System.Numerics

type Rectangle(pos: Vector2, sz: Vector2) =
    member val Position = pos
    member val Size = sz


type Image =

    abstract SubImage : Rectangle -> Image
    abstract Size : Vector2

type Transform =
    abstract Multiply : Vector2 -> Vector2
    abstract Multiply : Transform -> Transform

type GraphicsListener =
    abstract Update : int -> string option
    abstract Render : GraphicsManager -> unit

and GraphicsManager =
    abstract GraphicsListeners : GraphicsListener list with get, set
    abstract ScreenSize : Vector2
    abstract LoadImage : Stream -> Image
    abstract PushClip : Rectangle -> unit
    abstract PopClip : unit -> Rectangle option
    abstract PushTransform : Transform -> unit
    abstract PopTransform : unit -> Transform option
    abstract DrawImage : Image -> Vector2 -> unit
    abstract Start : (GraphicsManager -> unit) -> unit
    abstract Start : unit -> unit
    abstract IdentityTransform : Transform
    abstract RotationTransform : float32 -> Transform
    abstract TranslationTransform : Vector2 -> Transform
