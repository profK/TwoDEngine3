module TDE3ManagerInterfaces.InputDevices

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
    
    abstract Path: string with get
    
type InputDeviceInterface =
    abstract Controllers : Node list
    // gets changes since last update
    // The first map is added nodes
    // The second map is removed nodes
    // The third map is nodes whose value has changed
    abstract StateChanges : (Map<string,Node> * Map<string,Node> * Map<string,Node>)
