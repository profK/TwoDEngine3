namespace InputManagerGLFW

open GLFW
open GLFW
open TDE3ManagerInterfaces.InputDevices
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open TwoDEngine3.ManagerInterfaces.InputManager
open GLFW

type GLFWAxisNode(axisID:int,parentOpt:Node option,state) as this =
     interface Node with
         member val Name = $"Axis %d{axisID}" with get
         member val Parent = parentOpt with get
         member val Value = Axis(Analog(state))
         member val Path =
             match parentOpt with
             |Some parent -> parent.Path+"."+(this:>Node).Name
             |None -> (this:>Node).Name

type GLFWButtonNode(buttonID:int,parentOpt:Node option, state)as this =
     interface Node with
         member val Name = $"Button %d{buttonID}" with get
         member val Parent = parentOpt with get
         member val Value = Axis(Digital(state = InputState.Press))
          member val Path =
             match parentOpt with
             |Some parent -> parent.Path+"."+(this:>Node).Name
             |None -> (this:>Node).Name

type GLFWHatNode(hatID:int,parentOpt:Node option, state:Hat) as this =
    interface Node with
     member val Name = $"Hat %d{hatID}" with get
     member val Parent = parentOpt with get
     member val Value = Children(
         [0..3] |> Seq.map(fun id ->
             GLFWButtonNode(id,Some(this :> Node),
                            if ((uint8 state) &&& (uint8 2 >>> id)) = uint8 0  then
                                InputState.Release
                            else
                                InputState.Press))
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
            GLFWAxisNode(0,Some(this:>Node),0.0)
            GLFWAxisNode(1,Some(this:>Node),0.0); //Todo, actually set mouse deltas
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
        let axisPositions = Glfw.GetJoystickAxes(myID)
        let hatValues = Glfw.GetJoystickHats(int myID)
        let buttonValues = Glfw.GetJoystickButtons(myID)
        children <- 
        [ //axes
            [0..(Glfw.GetJoystickAxesCount(myID)-1)] |> Seq.map(fun num ->
                GLFWAxisNode(num,Some(this :> Node),float axisPositions.[num]) :> Node);
            [0..(Glfw.GetJoystickButtonsCount(myID)-1)] |> Seq.map(fun num ->
                GLFWButtonNode(num,Some(this :> Node),buttonValues.[num]) :> Node)
            [0..(Glfw.GetJoystickHatCount(int myID)-1)] |> Seq.map(fun num ->
                GLFWHatNode(num,Some(this :> Node),hatValues.[num]) :> Node)
        ] |> Seq.concat |> Seq.toList
    
    interface Node with
        member val Name = Glfw.GetJoystickName(joyID) with get
        member val Value = Children(children) with get
        member val Parent = parentOpt with get
        
        member val Path =
             match parentOpt with
             |Some parent -> parent.Path+"."+(this:>Node).Name
             |None -> (this:>Node).Name
             


           