namespace GraphicsMgrMonoGameDesktop

open Microsoft.Xna.Framework.Graphics

type DrawingState =
    | Begun
    | Ended

type DrawingStateManager(graphicsDevice: GraphicsDevice) =
    let graphics = graphicsDevice
    let mutable drawingState = Ended


    member val private spriteBatch = new SpriteBatch(graphics) with get, set
    member val private spriteEffect = new SpriteEffect(graphicsDevice) with get, set

    member this.BeginDrawing =
        this.EndDrawing

        this.spriteEffect <- new SpriteEffect(graphics)
        this.spriteBatch.Begin(effect = this.spriteEffect)
        drawingState <- Begun

    member this.SetTransform(xForm) =
        this.BeginDrawing // reset drawing state
        this.spriteEffect.TransformMatrix <- xForm // Note, this takes advantage of a sid effect, that Monogame
    // stores the TransformationMatrix by reference in the
    // sprite batch so I cna change it after the SB is
    // created

    member this.EndDrawing =
        match drawingState with
        | Begun ->
            this.spriteBatch.End()
            drawingState <- Ended
        | _ -> ()

    member this.SpriteBatch = this.spriteBatch
