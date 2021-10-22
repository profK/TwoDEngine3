module TwoDEngine3.ExampleLevel

open System.IO
open TwoDEngine3.LevelManagerInterface

type BouncyBall() as this =
    inherit AbstractLevelController()

    let ballImage =
        new FileStream("Assets/football_small.png", FileMode.Open)
        |> this.graphics.Value.LoadImage

    override this.Close() = printfn "BouncyBall closed"
    override this.Open() = printfn "BouncyBall opened"

    override this.RenderImpl deltams = printfn ("Bounce!")
