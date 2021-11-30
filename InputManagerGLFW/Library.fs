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
type GLFWKeyboardNode(parent) =
    interface Node with
        member this.Name = "Keyboard"
        member this.Parent = failwith "todo"
        member this.Value =
            Axis(Keyboard([]))

type GLFWMouseNode(parent) as this=
    
    interface Node with
        member this.Name = "Mouse"
        member this.Parent = failwith "todo"
        member this.Value = Children([
            GLFWAxisNode(0,Some(this:>Node))
            GLFWAxisNode(1,Some(this:>Node));
        ])

type GLFWJoyNode(joyID,parentOpt) as this =
    let myID = joyID
    let mutable children = List.Empty
    
    do this.MakeChildren()
    
    member this.MakeChildren() =
        children <- 
        [ //axes
            [0..(Glfw.GetJoystickAxesCount(myID)-1)] |> Seq.map(fun num ->
                GLFWAxisNode(num,Some(this :> Node)) :> Node);
            [0..(Glfw.GetJoystickButtonsCount(myID)-1)] |> Seq.map(fun num ->
                GLFWButtonNode(num,Some(this :> Node)) :> Node)
            [0..(Glfw.GetJoystickAxesCount(myID)-1)] |> Seq.map(fun num ->
                GLFWHatNode(num,Some(this :> Node)) :> Node)
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
   let mutable lastTree:Node list = List.empty 
   let mutable currentTree:Node list = List.empty
    
   interface GraphicsListener with
       member this.Render(graphics) =
           ()
       member this.Update(deltaTime) =
           Glfw.PollEvents()
           lastTree <-currentTree
           currentTree <-
               Joystick.GetValues()
               |> Array.fold (fun joylist (joystick:Joystick) ->
                    if Glfw.JoystickPresent(joystick) then
                        joystick::joylist
                    else
                        joylist
                   ) List.Empty
               |> List.map (fun joy ->
                    GLFWJoyNode(joy,None) :> Node
                   )
               |> List.append [GLFWMouseNode();GLFWKeyboardNode()]
           None // no errors
       
                    
   interface InputManager with
       member val Controllers = currentTree with get 
       member this.StateChanges =
           let changes = GetTreeChanges currentTree lastTree 
           //TODO cook lists to change statements
     
           