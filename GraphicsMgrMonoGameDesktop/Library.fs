namespace GraphicsMgrMonoGameDesktop


open ManagerRegistry
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open System.Numerics

type SysVec2 = System.Numerics.Vector2
type XnaVec2 = Microsoft.Xna.Framework.Vector2

type XnaVec3 = Microsoft.Xna.Framework.Vector3
type XnaRect = Microsoft.Xna.Framework.Rectangle
type XnaTransform = Microsoft.Xna.Framework.Matrix

type MGTransform(xnaTransform) =
    member val xform = xnaTransform
    interface Transform with
        member this.Multiply(var0: Transform) : Transform = failwith "todo"
        member this.Multiply(var0: Vector2) : Vector2 = failwith "todo"

type SpriteBatchState = Begun | Ended

[<Manager("A graphics manager based on Monogame Desktop",
          supportedSystems.Linux
          ||| supportedSystems.Mac
          ||| supportedSystems.Windows)>]
type GraphicsManagerMGDT() as this =
    inherit Game()
    let graphics = new GraphicsDeviceManager(this)
    
    let drawingState = lazy (DrawingStateManager(graphics.GraphicsDevice))

    let mutable transformStack: Transform list = List.Empty
    let mutable clipStack: Rectangle list = List.Empty
    member val private initFunc: (GraphicsManager -> unit) option = None with get, set
    override this.Initialize() = base.Initialize()

    override this.LoadContent() =
        base.LoadContent()
        let graphicsManager = this :> GraphicsManager
        graphicsManager.PushTransform graphicsManager.IdentityTransform 
        match this.initFunc with
        | Some cb -> cb (this)
        | None -> ()

    override this.Update(gameTime: GameTime) =
        let graphicsManager = (this :> GraphicsManager)

        graphicsManager.GraphicsListeners
        |> List.tryPick (fun listener -> listener.Update gameTime.ElapsedGameTime.Milliseconds)
        |> function
            | Some (error) ->
                printfn $"Quitting because %s{error}"
                this.Exit()
            | None -> ()

        base.Update gameTime

   
    override this.Draw gameTime =
        let graphicsManager = (this :> GraphicsManager)
        graphics.GraphicsDevice.Clear(Color.Black)
        drawingState.Force().BeginDrawing
        graphicsManager.GraphicsListeners
        |> List.iter (fun listener -> listener.Render(this))
        drawingState.Force().EndDrawing
        base.Draw gameTime

    interface GraphicsManager with
        override this.DrawImage image position =
            let mgImage:MonogameImage = image :?> MonogameImage

            let srcRect =
                XnaRect(int32 mgImage.origin.X, int32 mgImage.origin.Y, int32 mgImage.size.X, int32 mgImage.size.Y)

            let pos = XnaVec2(position.X, position.Y)
            drawingState.Force().SpriteBatch.Draw(mgImage.texture, pos, srcRect, Color.White)
            ()

        member val GraphicsListeners = List.Empty with get, set

        member this.LoadImage(istream) =
            let tex =
                Texture2D.FromStream(graphics.GraphicsDevice, istream)

            MonogameImage(tex, SysVec2(float32 0, float32 0), SysVec2(float32 tex.Width, float32 tex.Height)) :> Image

        member this.PopClip() = failwith "todo"
        member this.PopTransform() =
            let popResult = transformStack |> Stack.pop
            transformStack <- snd popResult
            drawingState.Force().SetTransform (transformStack.Head :?> MGTransform).xform
            fst popResult
        member this.PushClip(var0) = failwith "todo"
        member this.PushTransform(tform) =
            drawingState.Force().SetTransform (tform :?> MGTransform).xform
            transformStack <- transformStack |> Stack.push tform
   

        member this.ScreenSize =
            new SysVec2(float32 (graphics.PreferredBackBufferWidth), float32 (graphics.PreferredBackBufferHeight))

        member this.Start(initFunc: GraphicsManager -> unit) =
            this.initFunc <- Some initFunc
            (this :> Game).Run()
            ()

        member this.Start() =
            (this :> Game).Run()
            ()

        member this.IdentityTransform =
            (MGTransform XnaTransform.Identity) :> Transform
            
        member this.RotationTransform(rotRadians) =
            (MGTransform (XnaTransform.CreateRotationZ rotRadians)) :> Transform
        member this.TranslationTransform(v2) =
            (MGTransform (XnaTransform.CreateTranslation (XnaVec3 (v2.X, v2.Y, 0F )))):> Transform

and MonogameImage(Texture, ?Origin, ?Size) =
    member val internal texture: Texture2D = Texture
    member val internal origin: SysVec2 = defaultArg Origin (SysVec2(float32 0, float32 0))
    member val internal size: SysVec2 = defaultArg Size (SysVec2(float32 Texture.Width, float32 Texture.Height))

    interface Image with
        member this.Size = this.size

        member this.SubImage(rect) =
            MonogameImage(this.texture, rect.Position + this.origin, rect.Size) :> Image
