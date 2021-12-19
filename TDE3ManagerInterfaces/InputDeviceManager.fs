module TDE3ManagerInterfaces.InputDevices


type AxisEvent =
    | DigitalEvents of ButtonDownEvent:bool * ButtonUpEvent:bool
    | AnalogEvents of float
    | HatEvents of int
    | KeyboardEvents of KeyDownEvents:char list * KeyUpEvents:char list
    | DeltaEvents of float

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
    abstract StateChanges: unit ->
        (Map<string,AxisEvent> * Map<string,AxisEvent> * Map<string,AxisEvent>)
