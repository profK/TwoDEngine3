﻿module TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

open System.IO
open System.Numerics
open MathSharp
open MathSharp

type Rectangle =
    abstract Position : Vector2
    abstract Size : Vector2


type Image =

    abstract SubImage : Rectangle -> Image
    abstract Size : Vector2


type GraphicsListener =
    abstract Update : int -> string option
    abstract Render : GraphicsManager -> unit

and GraphicsManager =
    abstract GraphicsListeners : GraphicsListener list with get, set
    abstract ScreenSize : Vector2
    abstract LoadImage : Stream -> Image
    abstract PushClip : Rectangle -> unit
    abstract PopClip : unit -> Rectangle option
    abstract PushTransform : Matrix -> unit
    abstract PopTransform : unit -> Matrix option
    abstract DrawImage : Image-> Vector2 -> unit
    abstract Start : (GraphicsManager->unit)  -> unit
    abstract Start : unit  -> unit 
