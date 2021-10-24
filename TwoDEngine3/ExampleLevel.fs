module TwoDEngine3.ExampleLevel

open System.IO
open System.Numerics
open TwoDEngine3.LevelManagerInterface

type BouncyBall() as this =
    inherit AbstractLevelController()

    let ballImage =
        new FileStream("Assets/football_small.png", FileMode.Open)
        |> this.graphics.Value.LoadImage

    override this.Close() = printfn "BouncyBall closed"
    override this.Open() = printfn "BouncyBall opened"

    override this.RenderImpl graphics =
        let screenSize = this.graphics.Value.ScreenSize
        graphics.DrawImage ballImage (Vector2(screenSize.X/2f, screenSize.Y/2f))
        ()
