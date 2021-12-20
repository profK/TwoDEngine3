module TDE3ManagerInterfaces.InputDevices


type AxisEvent =
    | DigitalEvents of ButtonDownEvent:bool * ButtonUpEvent:bool
    | AnalogEvents of float
    | HatEvents of int
    | KeyboardEvents of KeyDownEvents:char list * KeyUpEvents:char list
    | DeltaEvents of float

let charListImplode (xs:char list) : string =
    let sb = System.Text.StringBuilder(xs.Length)
    xs |> List.iter (sb.Append >> ignore)
    sb.ToString()
let AxisEventToStr (axisEvt: AxisEvent) : string =
    match axisEvt with
    | DigitalEvents (down, up) -> $"Digital event (down=%b{down} up=%b{up})"
    | AnalogEvents value -> $"Analog Event (value=%f{value})"
    | HatEvents value -> $"Hat Event (value=%d{value})"
    | KeyboardEvents (downChars, upChars) ->
        $"Keyboard events (down=%s{charListImplode downChars} up=%s{charListImplode upChars})"
    | DeltaEvents value ->
        $"Delta event (%f{value})"
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
    abstract PollEvents: unit -> Map<string,AxisEvent>
