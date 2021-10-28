module TwoDEngine3.ExampleLevel

open System.IO
open System.Numerics
open TDE3ManagerInterfaces.TextRendererInterfaces
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

type BouncyBall() as this =
    inherit AbstractLevelController()

    let ballImage =
        new FileStream("Assets/football_small.png", FileMode.Open)
        |> this.graphics.Value.LoadImage
    let txtRenderer = ManagerRegistry.getManager<TextManager>().Value
    let Font = txtRenderer.LoadFont(txtRenderer.FontList.[0])
    let text = Font.MakeText("Ce n’est pas un ballon de football")

    override this.Close() =
        printfn "BouncyBall closed"
        let graphics = ManagerRegistry.getManager<GraphicsManager>()
        graphics.Value.PopTransform() |> ignore
        base.Close()

    override this.Open() =
        printfn "BouncyBall opened"
       

        base.Open()

    override this.RenderImpl graphics =
        let screenSize = this.graphics.Value.ScreenSize
      
        graphics.PushTransform(graphics.TranslationTransform(Vector2 (-100f, -100f)))

        graphics.DrawImage
            ballImage
            (Vector2((screenSize.X - ballImage.Size.X) / 2f, ((screenSize.Y - ballImage.Size.Y) / 2f)))
        graphics.PopTransform() |> ignore
        text |> txtRenderer.RenderText (Vector2 (0f,screenSize.Y/3f*2f))
        ()
