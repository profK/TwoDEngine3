namespace GraphicsMgrMonoGameDesktop


open ManagerRegistry
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open System.Numerics

type SysVec2 = System.Numerics.Vector2
type XnaVec2 = Microsoft.Xna.Framework.Vector2
type XnaRect = Microsoft.Xna.Framework.Rectangle

[<Manager("A graphics manager based on Monogame Desktop",
          supportedSystems.Linux ||| supportedSystems.Mac ||| supportedSystems.Windows)>]
type GraphicsManagerMGDT() as this=
    inherit Game()
   
    let graphics = new GraphicsDeviceManager(this)
    let spriteBatch = new SpriteBatch(graphics.GraphicsDevice)
    override this.Initialize() =
        base.Initialize()
    override this.Update (gameTime:GameTime) =
        let graphicsManager = (this :> GraphicsManager)
      
        graphicsManager.GraphicsListeners
        |> List.tryPick (fun listener -> listener.Update gameTime.ElapsedGameTime.Milliseconds)
        |> function
            | Some (error) ->
                printfn $"Quitting because %s{error}"
                this.Exit()
            | None -> ()
        spriteBatch.End()
        base.Update gameTime
    override this.Draw gameTime =
        let graphicsManager = (this :> GraphicsManager)
        graphics.GraphicsDevice.Clear(Color.Black)
        spriteBatch.Begin()
        graphicsManager.GraphicsListeners
        |> List.iter (fun listener -> listener.Render(this))
        spriteBatch.End()
        base.Draw gameTime
        
    interface GraphicsManager with
        override this.DrawImage image position =
            let mgImage = image :?> MonogameImage
            let srcRect = XnaRect(int32 mgImage.origin.Y,int32 mgImage.origin.Y,
                                    int32 mgImage.size.X,int32 mgImage.size.Y)
            let pos= XnaVec2(position.X,position.Y)
            spriteBatch.Draw (mgImage.texture, pos, srcRect, Color.White)
            ()
        
        member val GraphicsListeners = List.Empty with get,set
        member this.LoadImage(istream) =
            let tex = Texture2D.FromStream(graphics.GraphicsDevice, istream)
            MonogameImage(tex,SysVec2(float32 0,float32 0),
                          SysVec2(float32 tex.Width,float32 tex.Height)) :> Image
        member this.PopClip() = failwith "todo"
        member this.PopTransform() = failwith "todo"
        member this.PushClip(var0) = failwith "todo"
        member this.PushTransform(var0) = failwith "todo"
        member this.ScreenSize =
            new SysVec2(float32 graphics.PreferredBackBufferWidth,
                        float32 graphics.PreferredBackBufferHeight)
        member this.Start() = failwith "todo"

and MonogameImage(Texture,?Origin,?Size) =
    member val internal texture: Texture2D = Texture with get
    member val internal origin: SysVec2 =
        defaultArg Origin (SysVec2(float32 0,float32 0))
    member val internal size: SysVec2 =
        defaultArg Size (SysVec2(float32 Texture.Width,float32 Texture.Height))
    
    interface Image with     
        member this.Size = this.size
        member this.SubImage(rect) =
            MonogameImage(this.texture, rect.Position+this.origin,rect.Size) :> Image
      
        