module TwoDEngine3.ExampleLevel

open System
open System.IO
open System.Numerics
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
        | Digital b -> fprintf writer $"%s{spaces}Digital = %s{b.ToString()}"
        | Analog a -> fprintf writer $"%s{spaces}Analog = %s{a.ToString()}"
        | Keyboard k -> fprintf writer $"%s{spaces}Keyboard = %s{k.ToString()}"

        printfn ""// newline

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
        Font.MakeText("Ce nest pas un ballon de football")

    override this.Close() =
        printfn "BouncyBall closed"

        let graphics =
            ManagerRegistry.getManager<GraphicsManager> ()

        graphics.Value.PopTransform() |> ignore
        base.Close()

    override this.Open() =
        printfn "BouncyBall opened"
        


        base.Open()

    override this.RenderImpl graphics =
        let screenSize = this.graphics.Value.ScreenSize


        graphics.PushTransform(graphics.TranslationTransform -100f -100f)

        graphics.DrawImage
            ballImage
            (graphics.NewVector ((screenSize.X - ballImage.Size.X) / 2f)
                                ((screenSize.Y - ballImage.Size.Y) / 2f))

        graphics.PopTransform() |> ignore

        text
        |> txtRenderer.RenderText(graphics.NewVector  0f (screenSize.Y - 50.0f))

        ()
