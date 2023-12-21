module GraphicsManagerGLFW

open System
open System.IO
open System.Numerics
open System.Numerics
open System.Security.Cryptography
open System.Text
open System.Threading
open GlmNet
open StbImageSharp
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open gl
open OpenGL


type glImage = GLFW.Image
type tdeImage = TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface.Image

let NormalizeTo (value:float32) (max:float32) =
        (value/max)

    
type OglXform(glMatrix:Matrix4x4) =
   
    member val glMat:Matrix4x4 = glMatrix
    
    interface Transform with
        member this.Multiply (other:Transform):Transform =
            let otherXform = (other :?> OglXform).glMat
            OglXform(otherXform * this.glMat) :> Transform
            
        member this.Multiply (vec:Vector): Vector =
             Vector2.Transform(vec,this.glMat)


type OglImage(image:ImageResult,?rect,?texid) as this =
    let srcRect:Rectangle = (defaultArg rect
                  (Rectangle(Vector(0f,0f),
                        Vector(float32 image.Width,float32 image.Height)))) 

    member val src = srcRect with get
    member val img:ImageResult = image with get
   
    member val texID=uint32 0  with set, get 
    member this.BindImage() : uint32 =
        this.texID <- Gl.GenTexture() 
        Gl.BindTexture(TextureTarget.Texture2d, this.texID)
        // set the texture wrapping/filtering options (on the currently bound texture object)
        Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS,
                         TextureWrapMode.Repeat)
        Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT,
                         TextureWrapMode.Repeat)
        Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter,
                         TextureMinFilter.Linear)
        Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter,
                         TextureMagFilter.Linear)

        // load and generate the texture
        Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, this.img.Width,
                      this.img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, this.img.Data);
        Gl.GenerateMipmap(TextureTarget.Texture2d)
        this.texID
    
    member this.ReleaseImage() =
        Gl.DeleteTextures([| this.texID |])

    interface tdeImage with
        override this.SubImage rect =
            OglImage(this.img,rect) :> tdeImage
        override this.Size with get() =
            let oglImage = this :> OglImage                      
            Vector(this.src.Size.X, this.src.Size.Y) :> Vector
            
            
type GraphicsManagerGLFW()as this=
    //Create a window with the oop binding
    //
    let vertShaderCode =
        [|"""
    #version 330 core
    layout (location = 0) in vec3 aPos;
    layout (location = 1) in vec2 aTexCoord;

    uniform mat4 transform;
    out vec2 TexCoord;

    void main()
    {
        gl_Position = transform * vec4(aPos, 1.0) ;
        TexCoord = aTexCoord;
    }  
        """|]
    
    
    let fragShaderCode =                                    
        [| """
    #version 330 core
    out vec4 FragColor;
    in vec2 TexCoord;

    uniform sampler2D ourTexture;

    void main()
    {
        FragColor = texture(ourTexture, TexCoord);
    }
        """|]                                                  

   
    
    let mutable xformStack:Transform list = List.Empty
    let mutable clipStack:Rectangle list = List.Empty
    
    let mutable window : DeviceContextGLX.NativeWindow option = None
    
    member val shaderProgram:uint32 = 0u with get, set
                                                                            
    interface GraphicsManager with
        member this.DrawImage(img:tdeImage)  =
            let wbnds = window.Value.Bounds
            let sWidth = wbnds.Width
            let sHeight = wbnds.Height
            let xOfs = (img.Size.X/float32 sHeight)/2f
            let yOfs = (img.Size.X/float32 sWidth)/2f
            let oglImage = (img :?> OglImage)
            let texTl = [|
                NormalizeTo oglImage.src.Position.X (float32 oglImage.img.Width)
                (NormalizeTo oglImage.src.Position.Y (float32 oglImage.img.Height))
            |]
            
            let texLR = [| 
                (oglImage.src.Position.X+oglImage.src.Size.X)/(float32 oglImage.img.Width)
                ((oglImage.src.Position.Y+oglImage.src.Size.Y)/(float32 oglImage.img.Height))
            |]
            let vertexCoords =
                [|
                //position         texture  - Ys are reversed to get TL coord system rather than LL
                -xOfs;-yOfs;0f;    texTl.[0];texLR.[1];
                 xOfs;-yOfs;0f;    texLR.[0];texLR.[1];
                 xOfs;yOfs;0f;     texLR.[0];texTl.[1];
                 -xOfs;yOfs;0f;    texTl.[0];texTl.[1];
              
            |]
           
           // make vertex buffer
            let vbuff = [| uint32 0 |]
            Gl.GenBuffers(vbuff)
            Gl.BindBuffer(BufferTarget.ArrayBuffer,vbuff.[0])
            Gl.BufferData(BufferTarget.ArrayBuffer,(uint32) (sizeof<float32>*5*4), vertexCoords,
                          BufferUsage.StaticDraw)
            Gl.VertexAttribPointer(0u, 3, VertexAttribType.Float, false, 5*sizeof<float32>, 0);
            Gl.EnableVertexAttribArray(0u)
            Gl.VertexAttribPointer(1u, 2, VertexAttribType.Float, false, 5*sizeof<float32>,
                                   3* sizeof<float32>);
            Gl.EnableVertexAttribArray(1u)
           
            //make texture buffer
            let oglImage = img :?> OglImage
            let texID = oglImage.BindImage()
             
            Gl.UseProgram(this.shaderProgram)
            Gl.BindTexture(TextureTarget.Texture2d, texID)
            Gl.BindVertexArray(vbuff.[0])
            let transformLoc = Gl.GetUniformLocation(this.shaderProgram,"transform")
            let oglMat = match xformStack with
                         | [] -> Matrix4x4.Identity
                         | head::tail -> (head :?> OglXform).glMat
            Gl.UniformMatrix4f(transformLoc,1,false, oglMat)
            //draw
            Gl.DrawArrays(PrimitiveType.Quads, 0, 4)
            oglImage.ReleaseImage()
        member val GraphicsListeners:(GraphicsListener list) = List.Empty  with get, set
        member val IdentityTransform =
            OglXform(Matrix4x4.Identity) :> Transform
        member this.LoadImage(stream:Stream) =
            let image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            OglImage(image) :> tdeImage
        member this.PopClip() =
            match clipStack with
            | [] -> None
            | head::tail ->
                clipStack <- tail
                Some(head)
        member this.PopTransform() =
            match xformStack with
            | [] -> None
            | head::tail ->
                xformStack <- tail
                Some(head)
                
        member this.PeekTransform() =
            match xformStack with
            | [] -> None
            | head::tail ->
                Some(head)
        member this.PushClip(rect) = 
            clipStack <- rect::clipStack
        member this.PushTransform(xform) =
            (xformStack <- xform :: xformStack) |> ignore
        member this.PushMultTransform(xform) =
            let gm = (this :> GraphicsManager)
            let top = match xformStack with
                      | [] -> gm.IdentityTransform
                      | head::tail -> head
            gm.PushTransform (top.Multiply xform)    
        member this.RotationTransform(angle) =
             OglXform(Matrix4x4.CreateRotationZ(angle)) :> Transform
        member this.ScaleTransform  xScale yScale =
            OglXform(Matrix4x4.CreateScale(xScale,yScale,1f)) :> Transform
        member this.ScreenSize =
            let wsz = window.Value.Bounds
            let sWidth = float32(wsz.Width)
            let sHeight = float32(wsz.Height)
            Vector(float32 sWidth, float32 sHeight)
        member this.Start() =
            let CheckCompile shader =
                let mutable success = 99
                Gl.GetShader(shader, ShaderParameterName.CompileStatus,&success)
                if success=Gl.FALSE then
                    let mutable logSize = 512
                    let message = StringBuilder(512)
                    message.EnsureCapacity(512) |> ignore
                    Gl.GetShaderInfoLog(shader,512,&logSize, message)
                    printfn $"Compile Error: %s{message.ToString()}"
                    false
                else
                    printfn $"Shader Compiled" 
                    true
                    
            let mutable buffers = [| uint32 0 |]
            Gl.GenBuffers(buffers)
            Glfw.Init() |> ignore
            //Glfw.WindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
            //Glfw.WindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
            //Glfw.WindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
            window <- Some(new NativeWindow(800, 600, "Glfw test window"))  
            //Glfw.MakeContextCurrent(window.Value.)
            let vshader = Gl.CreateShader(ShaderType.VertexShader)
            Gl.ShaderSource(vshader,vertShaderCode)
            Gl.CompileShader(vshader)
            CheckCompile(vshader)
            let fshader =Gl.CreateShader(ShaderType.FragmentShader)
            Gl.ShaderSource(fshader,fragShaderCode)
            Gl.CompileShader(fshader)
            CheckCompile(fshader)
            this.shaderProgram <- Gl.CreateProgram()
            Gl.AttachShader(this.shaderProgram,fshader)
            Gl.AttachShader(this.shaderProgram,vshader)
            Gl.LinkProgram(this.shaderProgram)
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            Gl.Enable( EnableCap.Blend );
            let window = window.Value // convenience
           
            //Glfw.SetWindowCloseCallback(window,fun args -> window.Close()) |> ignore
            //You need to be notified if the size changed? 
            //Simply add a handler to the Size changed event
            //window.SizeChanged.Add (fun args -> printfn "Size changed! New width %A, new height %A" args.width args.height)
            while 
                (this :> GraphicsManager).GraphicsListeners 
                |> List.tryPick (fun listener ->
                        listener.Update
                            (this :> GraphicsManager)
                            (uint DateTime.Now.Millisecond))
                |> function
                   | Some (error) ->
                        printfn $"Quitting because %s{error}"
                        false
                   | None -> true
                do
                    Gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f)     
                    Gl.Clear(ClearBufferMask.ColorBufferBit|||ClearBufferMask.DepthBufferBit)
                    Gl.MatrixMode(MatrixMode.Modelview);
                    Gl.LoadIdentity();
                    Gl.Translate( 0.0, 0.0, -15.0 )
                    Gl.UseProgram(this.shaderProgram)  
                    (this :> GraphicsManager).GraphicsListeners 
                    |> Seq.iter(fun listener-> listener.Render(this) ) 
                    window.SwapBuffers
                    Glfw.PollEvents();
        member this.Start(userfunc) =
            userfunc(this)  
            (this :> GraphicsManager).Start()
            
        member this.TranslationTransform x y =
            let graphics = this :> GraphicsManager
            let screenSize = graphics.ScreenSize
            let oglXform = OglXform(
                Matrix4x4.CreateTranslation((float32 x)/screenSize.X,
                                            -(float32 y)/screenSize.Y,0f))
            oglXform :> Transform
        
    
        
       
    