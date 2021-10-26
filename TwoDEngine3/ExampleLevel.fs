module TwoDEngine3.ExampleLevel

open System.IO
open System.Numerics
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

type BouncyBall() as this =
    inherit AbstractLevelController()

    let ballImage =
        new FileStream("Assets/football_small.png", FileMode.Open)
        |> this.graphics.Value.LoadImage

    override this.Close() =
        printfn "BouncyBall closed"
        let graphics = ManagerRegistry.getManager<GraphicsManager>()
        graphics.Value.PopTransform() |> ignore
        base.Close()

    override this.Open() =
        printfn "BouncyBall opened"
        let graphics = ManagerRegistry.getManager<GraphicsManager>()
        graphics.Value.PushTransform(graphics.Value.TranslationTransform(Vector2 (100f, 200f)))
        base.Open()

    override this.RenderImpl graphics =
        let screenSize = this.graphics.Value.ScreenSize

        graphics.DrawImage
            ballImage
            (Vector2((screenSize.X - ballImage.Size.X) / 2f, ((screenSize.Y - ballImage.Size.Y) / 2f)))
            
        graphics.PushTransform(graphics.TranslationTransform(Vector2 (-100f, -100f)))

        graphics.DrawImage
            ballImage
            (Vector2((screenSize.X - ballImage.Size.X) / 2f, ((screenSize.Y - ballImage.Size.Y) / 2f)))
        ()
