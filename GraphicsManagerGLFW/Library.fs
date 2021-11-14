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
open glfw3
open OpenGL
open glfw3

type OglVector(vec:vec4)  =
    inherit Vector() 
    member this.oglVec = vec
   
    override this.X = this.oglVec.x
    override this.Y = this.oglVec.y
    override this.Plus (other:Vector) =
        let otherVec = (other :?> OglVector).oglVec
        OglVector(this.oglVec+otherVec) :> Vector
    new(x:float32, y:float32) as this =
        OglVector(vec4(x, y, 0f,1f))
    
type OglXform(glMatrix:mat4) =
   
    member val glMat:mat4 = glMatrix
    
    interface Transform with
        member this.Multiply (other:Transform):Transform =
            let otherXform = (other :?> OglXform).glMat
            OglXform(otherXform * this.glMat) :> Transform
            
        member this.Multiply (vec:Vector): Vector =
            let oglvec = (vec :?> OglVector).oglVec
            let newVec = this.glMat * oglvec 
            OglVector(newVec) :> Vector

type OglImage(image:ImageResult,?rect,?texid) as this =
   
    
    let MakeTexture() =
            let texid = Gl.GenTexture()
            Gl.BindTexture(TextureTarget.Texture2d,texid)
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS,
                             TextureWrapMode.Repeat)
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT,
                             TextureWrapMode.Repeat)
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter,
                             TextureMinFilter.LinearMipmapLinear)
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter,
                             TextureMagFilter.Linear)
            
            Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba,
                          image.Width, image.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte,
                          image.Data);
            Gl.GenerateMipmap(TextureTarget.Texture2d)
            Gl.ActiveTexture(TextureUnit.Texture0); // activate the texture unit first before binding texture
            Gl.BindTexture(TextureTarget.Texture2d, texid)
            texid
    let NormalizeTo (value:float32) (div:float32) =
        (value*2f/div)-1f
    member val texID = (defaultArg texid (MakeTexture())) with get
    member val src:Rectangle = (defaultArg rect
                  (Rectangle(OglVector(0f,0f),
                        OglVector(float32 image.Width,float32 image.Height))))
    member val img:ImageResult = image
   
    member val texCoord = ([|
        NormalizeTo ((this.src).Position.X) (float32 (this.img).Width)
        NormalizeTo ((this.src).Position.Y) (float32 (this.img).Height)
        NormalizeTo ((this.src).Position.X+this.src.Size.X) (float32 (this.img).Width)
        NormalizeTo ((this.src).Position.Y+this.src.Size.Y) (float32 (this.img).Height)
    |]) with get
    
    member val 
    interface Image with
       
        override this.SubImage rect =
            OglImage(this.img,rect) :> Image
        override this.Size with get() =
            let oglImage = this :> OglImage                      
            OglVector(this.src.Size.X, this.src.Size.Y) :> Vector
            
            
type GraphicsManagerGLFW()as this=
    //Create a window with the oop binding
    //
    let fragShaderCode =                                    
      [| """
#version 330
 
in vec2 texCoordV;
out vec4 colorOut;
 
void main() {
    colorOut = vec4(texCoordV, 0.0, 0.0);    
}
    """|]                                                  
    let vertShaderCode =
        [|"""
#version 330    
uniform mat4 pvm;
 
in vec4 position;
in vec2 texCoord;
 
out vec2 texCoordV;
 
void main() {
 
    texCoordV = texCoord;
    gl_Position = position;
}
        """|]
   
    
    let mutable xformStack = List.Empty
    let mutable clipStack = List.Empty
    
    let mutable window : Window option = None
    
    member val shaderProgram:uint32 = 0u with get, set
                                                                            
    interface GraphicsManager with
        member this.DrawImage(img:Image) (pos) =
            let wsz = window.Value.GetSize()
            let sWidth = float32(fst wsz)
            let sHeight = float32(snd wsz)
            let xOfs = (img.Size.X/sWidth)/2f
            let yOfs = (img.Size.X/sWidth)/2f
            let vertexCoords = [|
                -xOfs,-yOfs,0f,  xOfs,-yOfs,0f,
                xOfs,yOfs,0f,  -xOfs,yOfs,0f
                
            |]
           
            Gl.UseProgram(this.shaderProgram)
            
            
            
        member val GraphicsListeners:(GraphicsListener list) = List.Empty  with get, set
        member val IdentityTransform =
            OglXform(mat4.identity()) :> Transform
        member this.LoadImage(stream:Stream) =
            let image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            OglImage(image) :> Image
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
                
        member this.PushClip(rect) = 
            clipStack <- rect::clipStack
        member this.PushTransform(xform) =
            xformStack = xform :: xformStack |> ignore
        member this.RotationTransform(angle) =
             OglXform(glm.rotate(angle,
                                   vec3(0f,0f,1f))) :> Transform
        member this.ScreenSize =
            let gm = this :?> GraphicsManagerGLFW
            let mutable w = 0;
            let mutable h= 0;
            window.Value.GetSize(ref w, ref h)
            OglVector(float32 w,float32 h) :> Vector
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
            window <- Some(new Window(1024, 768, "Glfw test window"))  
            Glfw.MakeContextCurrent(window.Value)
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
            
            let window = window.Value // convenience
           
            Glfw.SetWindowCloseCallback(window,fun args -> window.Close()) |> ignore
            //You need to be notified if the size changed? 
            //Simply add a handler to the Size changed event
            //window.SizeChanged.Add (fun args -> printfn "Size changed! New width %A, new height %A" args.width args.height)
            while 
                (this :> GraphicsManager).GraphicsListeners 
                |> List.tryPick (fun listener -> listener.Update DateTime.Now.Millisecond)
                |> function
                   | Some (error) ->
                        printfn $"Quitting because %s{error}"
                        false
                   | None -> true
                do
                    Gl.ClearColor(0.0f, 0.0f, 1.0f, 1.0f)     
                    Gl.Clear(ClearBufferMask.ColorBufferBit)
                    Gl.MatrixMode(MatrixMode.Modelview);
                    Gl.LoadIdentity();
                    Gl.Translate( 0.0, 0.0, -15.0 )
                    Gl.UseProgram(this.shaderProgram)  
                    (this :> GraphicsManager).GraphicsListeners 
                    |> Seq.iter(fun listener-> listener.Render(this) ) 
                    Glfw.SwapBuffers(window)
                    Glfw.PollEvents();
        member this.Start(userfunc) =
            userfunc(this)  
            (this :> GraphicsManager).Start()
            
        member this.TranslationTransform x y =
            OglXform(glm.translate(mat4.identity(),
                                   vec3(float32 x,float32 y,0f))) :> Transform
        
        member this.NewVector x y =
            OglVector(x,y) :> Vector 
        
       
    