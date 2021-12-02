namespace InputManagerGLFW

open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open TwoDEngine3.ManagerInterfaces.InputManager
open GLFW

type GLFWAxisNode(axisID:int,parentOpt:Node option) as this =
     interface Node with
         member val Name = $"Axis %d{axisID}" with get
         member val Parent = parentOpt with get
         member val Value = Axis(Analog(0.0))
         member val Path =
             match parentOpt with
             |Some parent -> parent.Path+"."+(this:>Node).Name
             |None -> (this:>Node).Name

type GLFWButtonNode(buttonID:int,parentOpt:Node option)as this =
     interface Node with
         member val Name = $"Button %d{buttonID}" with get
         member val Parent = parentOpt with get
         member val Value = Axis(Digital(false))
          member val Path =
             match parentOpt with
             |Some parent -> parent.Path+"."+(this:>Node).Name
             |None -> (this:>Node).Name

type GLFWHatNode(hatID:int,parentOpt:Node option) as this =
    interface Node with
     member val Name = $"Hat %d{hatID}" with get
     member val Parent = parentOpt with get
     member val Value = Children(
         [0..3] |> Seq.map(fun id -> GLFWButtonNode(id,Some(this :> Node)))
         |> Seq.cast<Node> |> Seq.toList)
      member val Path =
             match parentOpt with
             |Some parent -> parent.Path+"."+(this:>Node).Name
             |None -> (this:>Node).Name
             
type GLFWKeyboardNode(parentOpt:Node option) as this =
    interface Node with
        member val Name = "Keyboard" with get
        member val Parent = parentOpt with get
        member val Value =
            Axis(Keyboard([]))
        member val Path =
             match parentOpt with
             |Some parent -> parent.Path+"."+(this:>Node).Name
             |None -> (this:>Node).Name
                 

type GLFWMouseNode(parentOpt:Node option) as this=
    
    interface Node with
        member this.Name = "Mouse"
        member this.Parent = parentOpt
        member this.Value = Children([
            GLFWAxisNode(0,Some(this:>Node))
            GLFWAxisNode(1,Some(this:>Node));
        ])
        
        member val Path =
             match parentOpt with
             |Some parent -> parent.Path+"."+(this:>Node).Name
             |None -> (this:>Node).Name
             

type GLFWJoyNode(joyID,parentOpt:Node Option) as this =
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
        
        member val Path =
             match parentOpt with
             |Some parent -> parent.Path+"."+(this:>Node).Name
             |None -> (this:>Node).Name
             


           