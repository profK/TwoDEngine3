module TDE3ManagerInterfaces.InputDevices


type AxisState =
    | DigitalState of bool
    | AnalogState of float
    | HatState of int
    | KeyboardState of char list
    | DeltaState of float

type AxisEnum =
    | Digital 
    | Analog 
    | Hat
    | Keyboard
    | Delta
and NodeValue =
    | Axis of AxisEnum
    | Children of Node list

and Node =
    abstract Name: string with get
    abstract Value: NodeValue with get
    abstract Parent: Node option with get
    
    abstract Path: string with get
    
type InputDeviceInterface =
    abstract Controllers : unit -> Node list
    // gets changes since last update
    // The first map is added nodes
    // The second map is removed nodes
    // The third map is nodes whose value has changed
    abstract PollState: unit -> Map<string,AxisState>
