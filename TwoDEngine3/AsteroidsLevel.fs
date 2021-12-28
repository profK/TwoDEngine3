module TwoDEngine3.AsteroidsLevel

open System.IO
open TDE3ManagerInterfaces.SceneGraphInterface
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open TwoDEngine3.SceneGraphNodes

type AsteroidsLevel() =
     inherit AbstractLevelController()
     let mutable ship = None
     let sceneGraph = ManagerRegistry.getManager<SceneGraphInterface>()
     
     
     override this.Open() =
        base.Open()
        use filestream = File.Open("Assets/asteriods-arcade.png", FileMode.Open)
        let atlas = this.graphics.Value.LoadImage filestream
                        
        let shipImage = atlas.SubImage (
                            Rectangle(
                                this.graphics.Value.NewVector 3f 2f,
                                this.graphics.Value.NewVector 25f 30f)
                         )
        let shipXform = this.graphics.Value.TranslationTransform 50f 50f 
        let shipSprite = Sprite(shipImage,shipXform,
                                this.graphics.Value.NewVector 0f 0f)
        sceneGraph.Value.AddChild(shipSprite)
        ()
        