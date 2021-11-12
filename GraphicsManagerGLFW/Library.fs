module GraphicsManagerGLFW

open System
open System.IO
open System.Numerics
open System.Numerics
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

type OglImage(image,?rect) =
    
    let img: ImageResult = image
    let src = defaultArg rect
                  (Rectangle(OglVector(0f,0f),
                        OglVector(float32 img.Width,float32 img.Height)))

    interface Image with
        override this.SubImage rect =
            OglImage(img,rect) :> Image
        override val Size =
            OglVector(float32 img.Width, float32 img.Height) :> Vector
type GraphicsManagerGLFW()as this=
    //Create a window with the oop binding
    let vertShaderCode = [|
        "#version 400\n"
        "in vec3 vp;\n"
        "void main() {\n"
        "   gl_Position = vec4(vp, 1.0);\n"
        "}"
    |]    
    let fragShaderCode = [|
        "#version 400\n"
        "out vec4 frag_colour;\n"
        "void main() {\n"
        "    frag_colour = vec4(0.5, 0.0, 0.5, 1.0);\n"
        "}"
    |]
    
     let mutable xformStack:Transform list = List.Empty
     let mutable clipStack:Rectangle list = List.Empty
    
     member val window = new Window(800, 600, "Glfw test window") 
     
    interface GraphicsManager with
        member this.DrawImage(var0) (var1) = failwith "todo"
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
            this.window.GetSize(ref w, ref h)
            OglVector(float32 w,float32 h) :> Vector
        member this.Start() =
            Glfw.Init() |> ignore
            let vshader = Gl.CreateShader(ShaderType.VertexShader)
            Gl.ShaderSource(vshader,vertShaderCode)
            Gl.CompileShader(vshader)
            let fshader =Gl.CreateShader(ShaderType.FragmentShader)
            Gl.ShaderSource(fshader,fragShaderCode)
            Gl.CompileShader(fshader)
            let shaderProgram = Gl.CreateProgram()
            Gl.AttachShader(shaderProgram,fshader)
            Gl.AttachShader(shaderProgram,vshader)
            Gl.LinkProgram(shaderProgram)
            
            let window = this.window // convenience
           
            Glfw.SetWindowCloseCallback(window,fun args -> window.Close()) |> ignore
            //You need to be notified if the size changed? 
            //Simply add a handler to the Size changed event
            window.SizeChanged.Add (fun args -> printfn "Size changed! New width %A, new height %A" args.width args.height)
            while 
                (this :> GraphicsManager).GraphicsListeners 
                |> List.tryPick (fun listener -> listener.Update DateTime.Now.Millisecond)
                |> function
                   | Some (error) ->
                        printfn $"Quitting because %s{error}"
                        false
                   | None -> true
                do
                    Gl.Clear(ClearBufferMask.ColorBufferBit|||ClearBufferMask.DepthBufferBit)
                    Gl.UseProgram(shaderProgram)
                    (this :> GraphicsManager).GraphicsListeners 
                    |> Seq.iter(fun listener-> listener.Render(this) )
                    Glfw.SwapBuffers(window)
                    Glfw.PollEvents();
        member this.Start(var0) = failwith "todo"
        member this.TranslationTransform x y =
            OglXform(glm.translate(mat4.identity(),
                                   vec3(float32 x,float32 y,0f))) :> Transform
        
        member this.NewVector x y =
            OglVector(x,y) :> Vector 
        
       
    