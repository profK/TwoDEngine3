module TwoDEngine3.ManagerInterfaces.InputManager

type AxisUnion =
    | DigitalAxis of bool
    | Analog of float
    | Keyboard of char list

type Axis =
    struct
          val axis : AxisUnion
          val path : string
    end

type InputManager =
    abstract PollAllAxes: unit -> Axis list
    abstract PollChangedAxes:  unit -> Axis list
    