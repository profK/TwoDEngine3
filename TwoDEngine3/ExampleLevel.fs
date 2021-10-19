module TwoDEngine3.ExampleLevel

open ManagerRegistry
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

type BouncyBall() =
   inherit AbstractLevelController ()
        override this.Close() =
            printfn "BouncyBall closed"
        override this.Open() =
            printfn "BouncyBall opened"
            
        override this.RenderImpl deltams =
            printfn("Bounce!")
        
            