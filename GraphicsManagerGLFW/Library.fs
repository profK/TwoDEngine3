module GraphicsManagerGLFW

open System
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
     
    interface GraphicsManager with
        member this.DrawImage(var0) (var1) = failwith "todo"
        member val GraphicsListeners:(GraphicsListener list) = List.Empty  with get, set
        member val IdentityTransform = failwith "todo"
        member this.LoadImage(var0) = failwith "todo"
        member this.PopClip() = failwith "todo"
        member this.PopTransform() = failwith "todo"
        member this.PushClip(var0) = failwith "todo"
        member this.PushTransform(var0) = failwith "todo"
        member this.RotationTransform(var0) = failwith "todo"
        member this.ScreenSize = failwith "todo"
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
            use window = new Window(800, 600, "Glfw test window")
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
        member this.TranslationTransform(var0) = failwith "todo"
       
    