module TwoDEngine3.AsteroidsLevel

open System.IO
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open TwoDEngine3.RenderGraphInterface
open TwoDEngine3.SceneGraphNodes


type AsteroidsLevel() =
     inherit AbstractLevelController()
     let mutable ship = None
    
     
     
     override this.Open() =
        base.Open()
        use filestream = File.Open("Assets/asteroids-arcade.png", FileMode.Open)
        let atlas = this.graphics.Value.LoadImage filestream
                        
        let shipImage = atlas.SubImage (
                            Rectangle(
                                Vector(3f, 2f),
                                Vector(25f, 30f)
                            )
                         )
        RENDERTREE this.graphics.Value [
            SPRITE shipImage []
        ]
        |> List.iter(fun node -> node.Render this.graphics.Value.IdentityTransform)
       
        ()
        