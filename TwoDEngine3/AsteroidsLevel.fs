module TwoDEngine3.AsteroidsLevel

open SceneGraph2D
open TDE3ManagerInterfaces
open TDE3ManagerInterfaces.SceneGraphInterface
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface

type AsteroidsLevel() =
     inherit AbstractLevelController()
     let mutable ship = None
     let sceneGraph = ManagerRegistry.getManager<SceneGraph2D>
     
     override this.Open() =
        base.Open()
        let atlas = this.graphics.Value.LoadImage "asteriods-arcade.png"
        let shipImage = atlas.SubImage (Rectangle (Vector 3 2) (Vector 25 30 ))
        ship <- sceneGraph.
        sceneGraph.AddSprite(ship)
        
        