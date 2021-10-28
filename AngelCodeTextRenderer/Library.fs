namespace AngelCodeTextRenderer

open System.IO
open Cyotek.Drawing.BitmapFont
open ManagerRegistry
open TDE3ManagerInterfaces.TextRendererInterfaces
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

[<Manager("Text renderer that uses angelcode bitmap fonts",
          supportedSystems.Windows
          ||| supportedSystems.Mac
          ||| supportedSystems.Windows)>]
type AngelCodeTextRenderer =
    interface TextManager with
        member this.FontList =
            Directory.GetFiles("AngelcodeFonts") |> Array.toList
            
        member this.LoadFont(fontName) =
            BitmapFontLoader.LoadFontFromFile("AngelcodeFonts/"+fontName)
            |> AngelCodeFont
            :> Font
           
        member this.RenderText (var0) (var1) = failwith "todo"
        
and AngelCodeFont(bmFont) as this =
    let bitmapFont = bmFont
    let graphics = ManagerRegistry.getManager<GraphicsManager>();
    //TODO build image dict

  
    
    interface Font with
        member this.MakeText(var0) = failwith "todo"
        member this.Name = failwith "todo"
        member this.Size = failwith "todo"
