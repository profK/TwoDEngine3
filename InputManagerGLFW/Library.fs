module InputManagerGLFW

open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open TwoDEngine3.ManagerInterfaces.InputManager
open GLFW

type GLFWAxisNode(axisID:int,parentOpt:Node option) =
     interface Node with
         member val Name = $"Axis %d{axisID}" with get
         member val Parent = parentOpt with get
         member val Value = Axis(Analog(0.0))

type GLFWButtonNode(buttonID:int,parentOpt:Node option) =
     interface Node with
         member val Name = $"Button %d{buttonID}" with get
         member val Parent = parentOpt with get
         member val Value = Axis(Digital(false))

type GLFWHatNode(hatID:int,parentOpt) as this =
    interface Node with
     member val Name = $"Hat %d{hatID}" with get
     member val Parent = parentOpt with get
     member val Value = Children(
         [0..3] |> Seq.map(fun id -> GLFWButtonNode(id,Some(this :> Node)))
         |> Seq.cast<Node> |> Seq.toList)
    

type GLFWJoyNode(joyID,parentOpt) as this =
    let mutable axisCount:int =0
    do Glfw.GetJoystickAxes(joyID,&axisCount) |> ignore
    //let mutable hatCount:int =0
    //do Glfw.GetJoystickHats(joyID,&hatCount) |> ignore
    let mutable buttonCount:int = 0
    do Glfw.GetJoystickButtons(joyID,&buttonCount) |> ignore
    let children =
        [ //axes
            [0..axisCount-1] |> Seq.map(fun num ->
                GLFWAxisNode(num,Some(this :> Node)) :> Node);
          //  [0..hatCount-1] |> Seq.map(fun num -> GLFWHatNode(num,Some(this)));
            [0..buttonCount-1] |> Seq.map(fun num ->
                GLFWButtonNode(num,Some(this :> Node)) :> Node)
        ] |> Seq.concat |> Seq.toList
    
    interface Node with
        member val Name = Glfw.GetJoystickName(joyID) with get
        member val Value = Children(children) with get
        member val Parent = parentOpt with get

type InputManagerGLFW() as this =
   let graphics =
        ManagerRegistry
            .getManager<GraphicsManager>()
                .Value
                    
   do graphics.GraphicsListeners <- (this:>GraphicsListener) :: graphics.GraphicsListeners
    
   interface GraphicsListener with
       member this.Render(graphics) =
           ()
       member this.Update(deltaTime) =
           Glfw.PollEvents()
           None // no errors
           
 
       
                    
   interface InputManager with
      (* member this.Controllers =
           Joystick.GetValues() |> Seq.choose(fun joy ->
                 Glfw.JoystickPresent(joy)
               )
           |> Seq.iter (fun joy ->
                 let node = Node()
               ) *)
           
       member this.ListenTo(var0) = failwith "todo"
       member this.StateChanges = failwith "todo"