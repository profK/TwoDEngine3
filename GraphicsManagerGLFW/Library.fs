module GraphicsManagerGLFW

open System.Numerics
open System.Threading
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open glfw3
open OpenGL
open glfw3

type OglXform() =
    interface Transform with
        member this.Multiply (other:Transform):Transform = failwith "todo"
        member this.Multiply (vec:Vector2): Vector2 = failwith "todo"

type GraphicsManagerGLFW() =
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

    
    let init =
        Glfw.Init() |> ignore
        let vshader = Gl.CreateShader(ShaderType.VertexShader)
        Gl.ShaderSource(vshader,vertShaderCode)
        Gl.CompileShader(vshader)
        let fshader =Gl.CreateShader(ShaderType.FragmentShader)
        Gl.ShaderSource(fshader,fragShaderCode)
        Gl.CompileShader(fshader)
        use window = new Window(800, 600, "Glfw test window")
        Glfw.SetWindowCloseCallback(window,fun args -> window.Close()) |> ignore
        //You need to be notified if the size changed? 
        //Simply add a handler to the Size changed event
        window.SizeChanged.Add (fun args -> printfn "Size changed! New width %A, new height %A" args.width args.height)
        while (not (window.ShouldClose())) do
            Thread.Sleep(500)
    do init
  
    
    interface GraphicsManager with
        member this.DrawImage(var0) (var1) = failwith "todo"
        member val GraphicsListeners = List.Empty  with get, set
        member val IdentityTransform = failwith "todo"
        member this.LoadImage(var0) = failwith "todo"
        member this.PopClip() = failwith "todo"
        member this.PopTransform() = failwith "todo"
        member this.PushClip(var0) = failwith "todo"
        member this.PushTransform(var0) = failwith "todo"
        member this.RotationTransform(var0) = failwith "todo"
        member this.ScreenSize = failwith "todo"
        member this.Start() = failwith "todo"
        member this.Start(var0) = failwith "todo"
        member this.TranslationTransform(var0) = failwith "todo"
       
    