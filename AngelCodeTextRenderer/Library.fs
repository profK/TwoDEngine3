namespace AngelCodeTextRenderer

open System.IO
open Cyotek.Drawing.BitmapFont
open ManagerRegistry
open TDE3ManagerInterfaces.TextRendererInterfaces

[<Manager("Text renderer that uses angelcode bitmap fonts",
          supportedSystems.Windows
          ||| supportedSystems.Mac
          ||| supportedSystems.Windows)>]
type AngelCodeTextRenderer =
    interface TextManager with
        member this.FontList =
            Directory.GetFiles("Fonts")
            |> Array.map
                (function
                | fname ->
                    BitmapFontLoader.LoadFontFromFile(fname)
                    |> AngelCodeFont
                    :> Font)
            |> Array.toList

        member this.RenderText (var0) (var1) = failwith "todo"

and AngelCodeFont(bmFont) =
    let bitmapFont = bmFont

    interface Font with
        member this.MakeText(var0) = failwith "todo"
        member this.Name = failwith "todo"
        member this.Size = failwith "todo"
