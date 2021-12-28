module TwoDEngine3.ExampleLevel

open System
open System.IO
open System.Numerics
open System.Threading
open TDE3ManagerInterfaces.InputDevices
open TDE3ManagerInterfaces.TextRendererInterfaces
open TwoDEngine3.LevelManagerInterface
open TwoDEngine3.ManagerInterfaces
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open TwoDEngine3.ManagerInterfaces.InputManager

let rec PrintControl (node: Node, writer: TextWriter, indent: int) =
    let spaces = new string (' ', indent)
    fprintf writer $"%s{spaces + (node.Name)}"

    match node.Value with
    | Children children ->
        printfn ""// new line
        children
        |> Seq.iter (fun child -> PrintControl(child, writer, indent + 4))
    | Axis axis ->
        let spaces = new string (' ', indent + 4)

        match axis with
        | Digital -> fprintf writer $"%s{spaces}Digital "
        | Analog -> fprintf writer $"%s{spaces}Analog "
        | Keyboard -> fprintf writer $"%s{spaces}Keyboard "

        printfn ""// newline

let implode (xs:char list) =
  let sb = System.Text.StringBuilder(xs.Length)
  xs |> List.iter (sb.Append >> ignore)
  sb.ToString()  
type BouncyBall() as this =
    inherit AbstractLevelController()

    let ballImage =
        new FileStream("Assets/football_small.png", FileMode.Open)
        |> this.graphics.Value.LoadImage


    let txtRenderer =
        ManagerRegistry.getManager<TextManager>().Value

    let Font =
        txtRenderer.LoadFont(txtRenderer.FontList.[0])

    let text =
        Font.MakeText("BBBBB bbbbb")

    override this.Close() =
        printfn "BouncyBall closed"

        let graphics =
            ManagerRegistry.getManager<GraphicsManager> ()

        graphics.Value.PopTransform() |> ignore
        base.Close()

    override this.Open() =
        
        let rec PrintControllers (controllerList:Node list) (indent:string) =
            controllerList
            |> Seq.iter(fun controller ->
                    printfn $"%s{indent}%s{controller.Name}"
                    let newIndent = indent+"  "
                    match controller.Value with
                    | Axis axis ->
                        match axis with
                        | Digital -> printfn $"{newIndent}Digitial Axis"
                        | Analog ->  printfn $"%s{newIndent}Analog Axis"
                        | Keyboard -> printfn $"%s{newIndent}Keyboard Axis"
                    | Children c -> PrintControllers c newIndent     
                )
        printfn "BouncyBall opened"
        
        let im = ManagerRegistry.getManager<InputDeviceInterface>()
        match im with
        | None -> printfn "No Input Manager found" |> ignore
        | Some im -> PrintControllers (im.Controllers()) "" |> ignore


        base.Open()
        
   

    override this.UpdateImpl graphics timeDelta =
        Thread.Sleep(5000)
        let im = ManagerRegistry.getManager<InputDeviceInterface>()
        match im with
        | None ->
            printfn "No Input Manager found" 
        | Some img ->
            img.PollState()
            |> Map.iter (fun (key:string) (value:AxisState) ->
                match value with
                | DigitalState b ->
                    printfn $"%s{key} : Digital(%b{b})" 
                | AnalogState v ->
                    printfn $"%s{key} : Analog(%f{v})"
                | DeltaState v ->
                    printfn $"%s{key} : Delta(%f{v})"
                | HatState h ->
                    printfn $"%s{key} : Hat(%d{h})"
                | KeyboardState chars ->
                    let s = implode(chars)
                    printfn $"%s{key} : Keyboard(%s{s})"
            )
        |> ignore
        None // no errors
    override this.RenderImpl graphics =
        let screenSize = this.graphics.Value.ScreenSize


        graphics.PushTransform(graphics.TranslationTransform -100f -100f)
        
        let subImage = ballImage.SubImage(
            Rectangle(
                Vector (ballImage.Size.X/4f, ballImage.Size.Y/4f),
                Vector (ballImage.Size.X/2f, ballImage.Size.Y/2f)
            )
        )

        graphics.DrawImage subImage
           

        graphics.PopTransform() |> ignore
        
        let texXform = (graphics.TranslationTransform 0f 50f).
                            Multiply(
                                graphics.ScaleTransform 4f 4f)
        
        graphics.PushTransform(texXform)

        text
        |> txtRenderer.RenderText

        graphics.PopTransform()
        
        ()
