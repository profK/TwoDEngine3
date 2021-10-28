namespace AngelCodeTextRenderer

open System.IO
open System.Numerics
open Cyotek.Drawing.BitmapFont
open ManagerRegistry
open TDE3ManagerInterfaces.TextRendererInterfaces
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open FSharp.Collections

[<Manager("Text renderer that uses angelcode bitmap fonts",
          supportedSystems.Windows
          ||| supportedSystems.Mac
          ||| supportedSystems.Windows)>]
type AngelCodeTextRenderer() =
    interface TextManager with
        member this.FontList =
            Directory.GetFiles("AngelcodeFonts")
            |> Array.toList

        member this.LoadFont(fontName) =
            BitmapFontLoader.LoadFontFromFile(fontName)
            |> AngelCodeFont
            :> Font

        member this.RenderText (pos: Vector2) (textObj) =
            let font: AngelCodeFont = textObj.Font :?> AngelCodeFont

            let graphics =
                ManagerRegistry
                    .getManager<GraphicsManager>()
                    .Value
           
            textObj.Text
            |> Seq.fold 
                (fun state char ->
                    let pos = fst state
                    let lastChar = snd state
                    let acChar: Character = font.GetCharacter char
                    let acImage: Image = font.GetPage(acChar.TexturePage)

                    let charImage =
                        acImage.SubImage(
                            Rectangle(
                                Vector2(float32 acChar.X, float32 acChar.Y),
                                Vector2(float32 acChar.Width, float32 acChar.Height)
                            )
                        )

                    graphics.DrawImage charImage
                        (pos + Vector2(float32 acChar.XOffset,float32 acChar.YOffset))
                    let kern = font.GetKern(lastChar,char)
                    (Vector2(pos.X+ float32 acChar.Width+float32 kern, pos.Y),char)
                ) (pos,'\n') |> ignore
            ()
                


and AngelCodeFont(bmFont)  =
    let bitmapFont = bmFont

    let graphics =
        ManagerRegistry.getManager<GraphicsManager> ()
  
    let pages =
        bmFont.Pages
        |> Array.fold
            (fun (pageMap: Map<int, Lazy<Image>>) page ->
                let fileStream =
                    File.Open(page.FileName, FileMode.Open)

                let lazyImage =
                    lazy (graphics.Value.LoadImage fileStream)

                Map.add page.Id lazyImage pageMap)
            Map.empty

    member this.GetPage(id) = pages.[id].Force()
    member this.GetCharacter char = bitmapFont.Characters.[char]
    
    member this.GetKern (last,curr) = bitmapFont.GetKerning(last,curr)

    interface Font with
        member this.MakeText(text) = AngelCodeText(text, this) :> Text
        member val Name = bmFont.FamilyName
        member val Size = bmFont.FontSize

and AngelCodeText(txt: string, fnt: AngelCodeFont) =
    let text = txt
    let font = fnt

    interface Text with
        member val Text = text
        member this.Font = font :> Font
