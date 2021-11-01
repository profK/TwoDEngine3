module TwoDEngine3.ManagerInterfaces.InputManager


type AxisUnion =
    | Digital of bool
    | Analog of float
    | Keyboard of char list

and NodeValue =
    | Axis of AxisUnion
    | Children of Node list

and Node(name: string, value: NodeValue, parent:Node option) =
    member val Name: string = name
    member val Value: NodeValue = value with get, set
    member val Parent: Node option = parent



type InputManager =
    abstract controllerRoot : Node
    
    abstract ListenTo: Node -> Node
    
    abstract StateChanges : Node seq 
