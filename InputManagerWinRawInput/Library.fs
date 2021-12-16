module InputManagerWinRawInput

open System
open System.Threading
open RawInputLight
open TwoDEngine3.ManagerInterfaces.InputManager
open Windows.Win32.Devices.HumanInterfaceDevice



 type AxisNode(parent:Node,name) =
        interface Node with
            member val Name:string = name with get
            member val Parent: Node option = Some(parent) with get
            member val Path:string=parent.Path+"."+name with get
            member val Value= Axis(Analog(0)) // value is ignored
            
type ButtonNode(parent:Node,name) =
     interface Node with
        member val Name:string = name with get
        member val Parent: Node option = Some(parent) with get
        member val Path:string=parent.Path+"."+name with get
        member val Value= Axis(Digital(false)) // value is ignored
                
type MouseNode(devInfo:DeviceInfo) as this=
    let name = devInfo.Names.Product
    
    interface Node with
        member val Name:string = name with get
        member val Parent: Node option = None with get
        member val Path:string=name with get
        member val Value= Children(
                [AxisNode(this,"deltaX");AxisNode(this,"deltaY");AxisNode(this,"deltaWheel")
                 ButtonNode(this,"button1");ButtonNode(this,"button2");ButtonNode(this,"button3")
                 ButtonNode(this,"button4")]
            ) with get
        
type KeyboardNode(devInfo:DeviceInfo) as this =
    let name = devInfo.Names.Product
    
    interface Node with
        member val Name:string = name with get
        member val Parent: Node option = None with get
        member val Path:string=name with get
        member val Value= Axis(Keyboard([]))
            
type JoystickNode(devInfo:DeviceInfo) as this =
    let MakeUsage(usage) =
         Microsoft.FSharp.Core.LanguagePrimitives.
            EnumOfValue<uint, HIDDesktopUsages>(uint usage)
    let MakeButtonNodes (buttonCaps:HIDP_BUTTON_CAPS array) =
        buttonCaps
        |> Array.fold(fun state (buttonCap:HIDP_BUTTON_CAPS) ->
            match buttonCap.IsRange.Value with
            | 0uy -> //FALSE
                ButtonNode(this, MakeUsage(buttonCap.Anonymous.NotRange.Usage).ToString()):> Node ::state // false
            | _ -> // TRUE
                [buttonCap.Anonymous.Range.UsageMin..buttonCap.Anonymous.Range.UsageMax]
                |> List.fold(fun state usageNum ->
                        ButtonNode(this,MakeUsage(usageNum).ToString()):>Node ::state
                    ) state
            ) List.Empty
        
    let MakeValueNodes (valueCaps:HIDP_VALUE_CAPS array) =
        valueCaps
        |> Array.fold(fun state (valueCap:HIDP_VALUE_CAPS) ->
            match valueCap.IsRange.Value with
            | 0uy -> //FALSE
                AxisNode(this, MakeUsage(valueCap.Anonymous.NotRange.Usage).ToString()):> Node ::state // false
            | _ -> // TRUE
                [valueCap.Anonymous.Range.UsageMin..valueCap.Anonymous.Range.UsageMax]
                |> List.fold(fun state usageNum ->
                        AxisNode(this,MakeUsage(usageNum).ToString()):>Node ::state
                    ) state
            ) List.Empty
        
    let name = devInfo.Names.Product
    
    interface Node with
        member val Name:string = name with get
        member val Parent: Node option = None with get
        member val Path:string=name with get
        member val Value= Children(
            [
                devInfo.ButtonCaps
                |> MakeButtonNodes
                devInfo.ValueCaps
                |> MakeValueNodes
            ]
            |> List.concat
            )

type InputManagerWinRawInput() as this =
       let messagePumpThread =
           Thread(ThreadStart(this.messagePump)).Start()
       
       member val RawInput: RawInput option = None with get, set
       
       [<STAThread>]
       member this.messagePump():unit =
           NativeAPI.OpenWindow()
           |> fun wrapper ->
               this.RawInput <- Some(RawInput(wrapper))
               NativeAPI.RefreshDeviceInfo();
               NativeAPI.MessagePump(wrapper)
               
       interface InputManager with
           member val Controllers =
               NativeAPI.GetDevices()
               |> Array.fold(fun state (devInfo:DeviceInfo) ->
                        if (devInfo.DeviceCaps.UsagePage = uint16(1)) then
                            let usage:HIDDesktopUsages =
                                Microsoft.FSharp.Core.LanguagePrimitives.
                                    EnumOfValue<uint, HIDDesktopUsages>(
                                        uint devInfo.DeviceCaps.Usage)
                            match usage  with
                            | HIDDesktopUsages.GenericDesktopMouse ->
                                MouseNode(devInfo):>Node :: state
                            | HIDDesktopUsages.GenericDesktopKeyboard ->
                                KeyboardNode(devInfo):>Node :: state
                            | HIDDesktopUsages.GenericDesktopJoystick ->
                                JoystickNode(devInfo):>Node :: state
                            | _ -> state
                        else
                            state
                   ) List.Empty
           member val StateChanges = failwith "todo"
       
