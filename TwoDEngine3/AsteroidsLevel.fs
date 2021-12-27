module TwoDEngine3.AsteroidsLevel


open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

type AsteroidsLevel() =
     inherit AbstractLevelController()
     let mutable ship = None
     let sceneGraph = ManagerRegistry.getManager<SceneGraph>
     
     override this.Open() =
        base.Open()
        let atlas = this.graphics.Value.LoadImage "asteriods-arcade.png"
        let shipImage = atlas.SubImage (Rectangle (Vector 3 2) (Vector 25 30 ))
        ship <- s
        sceneGraph.AddSprite(ship)
        
        