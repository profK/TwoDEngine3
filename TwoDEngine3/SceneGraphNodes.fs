module TwoDEngine3.SceneGraphNodes

open TDE3ManagerInterfaces.SceneGraphInterface
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface


type Sprite(img:Image,xform:Transform,veloc:Vector) =
    let image= img
    let transform = xform
    let velocity =veloc
    
    interface SceneGraphObjectInterface with
        member this.Render(graphics) =
            graphics.PushMultTransform xform
            graphics.DrawImage image
            graphics.PopTransform |> ignore
        member this.Update graphics deltaT =
            let move = Vector
                           (velocity.X*float32 deltaT,
                            velocity.Y*float32 deltaT)
            let moveXform = graphics.TranslationTransform move.X move.Y               
            Some(Sprite(img,xform.Multiply moveXform,velocity))
            

