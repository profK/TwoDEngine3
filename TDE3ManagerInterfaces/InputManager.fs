module TwoDEngine3.ManagerInterfaces.InputManager


type AxisUnion =
    | Digital of bool
    | Analog of float
    | Hat of int
    | Keyboard of char list

and NodeValue =
    | Axis of AxisUnion
    | Children of Node list

and Node =
    abstract Name: string with get
    abstract Value: NodeValue with get
    abstract Parent: Node option with get



type InputManager =
    abstract Controllers : Node list
    
    abstract ListenTo: Node -> unit
    
    abstract StateChanges : Node seq 
