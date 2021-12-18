module InputManagerWinRawInput

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Threading
open RawInputLight
open TDE3ManagerInterfaces.InputDevices
open TwoDEngine3.ManagerInterfaces.InputManager
open Windows.Win32.Devices.HumanInterfaceDevice
open Windows.Win32.Foundation
 
type AxisNode(parent:Node,name) =
        interface Node with
            member val Name:string = name with get
            member val Parent: Node option = Some(parent) with get
            member this.Path:string=parent.Path+"."+name 
            member val Value= Axis(Analog(0)) // value is ignored
            
type ButtonNode(parent:Node,name) =
     interface Node with
        member val Name:string = name with get
        member val Parent: Node option = Some(parent) with get
        member this.Path:string=parent.Path+"."+name 
        member val Value= Axis(Digital(false)) // value is ignored
                
type MouseNode(devInfo:DeviceInfo) as this=
    let name = devInfo.Names.Product
    
    interface Node with
        member val Name:string = name with get
        member val Parent: Node option = None with get
        member this.Path:string=name 
        member val Value= Children(
                [AxisNode(this,"deltaX");AxisNode(this,"deltaY");AxisNode(this,"deltaWheel")
                 ButtonNode(this,"button0");ButtonNode(this,"button1");ButtonNode(this,"button2")
                 ButtonNode(this,"button3")]
            ) with get
        
type KeyboardNode(devInfo:DeviceInfo) as this =
    let name = devInfo.Names.Product
    
    interface Node with
        member val Name:string = name with get
        member val Parent: Node option = None with get
        member this.Path:string=name
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
        member this.Path:string=name 
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
       let mutable rawInput: RawInput option = None
       let mutable oldStateMap = Map.empty
       let  axisStateCollector =
           new ConcurrentDictionary<string,AxisUnion>()
           
       let doKbEvent (devh:HANDLE) (asc:uint16) (keystate:KeyState):unit =
            let devInfo:Nullable<DeviceInfo> = NativeAPI.GetDeviceInfo(devh)
            if devInfo.HasValue then
                match keystate with
                | KeyState.KeyDown ->
                    axisStateCollector.AddOrUpdate(
                        devInfo.Value.Names.Product,
                        Keyboard([char asc]),
                        (fun (name:string) (oldAxisUnion:AxisUnion)->
                            let (Keyboard oldList) = oldAxisUnion
                            Keyboard(
                                (char asc)::oldList
                            )
                        )
                    ) |> ignore
                | KeyState.KeyUp ->
                    axisStateCollector.AddOrUpdate(
                        devInfo.Value.Names.Product,
                        Keyboard([]),
                        (fun (name:string) (oldAxisUnion:AxisUnion) ->
                            let (Keyboard oldList) = oldAxisUnion 
                            Keyboard(
                                    oldList
                                    |> List.except([char asc])
                                )
                            )
                        )|> ignore
            else
                ()
       let DeltaAnalogAxis (name:string, value:float):AxisUnion =
           axisStateCollector.AddOrUpdate(
                   name,
                    Analog(value),
                    (fun (name:string) (currentAxis:AxisUnion)->
                        let (Analog currentVal) = currentAxis
                        Analog(currentVal+value)
                    )
           )
                    
       let SetAnalogAxis (name:string, value:float):AxisUnion =
           axisStateCollector.AddOrUpdate(
                   name,
                    Analog(value),
                    (fun (name:string) (currentAxis:AxisUnion)->
                        Analog(value)
                    )
                )
       let SetDigitalAxis(name:string, value:bool) =
           axisStateCollector.AddOrUpdate(
                   name,
                    Digital(value),
                    (fun (name:string) (currentAxis:AxisUnion)->
                        let (Digital currentVal) = currentAxis
                        Digital(value)
                    )
                )
           
       let doMouseEvent (devh:HANDLE) (dx:int) (dy:int) (buttons:UInt32)
            (dWheel:int) =
            let devInfo:Nullable<DeviceInfo> = NativeAPI.GetDeviceInfo(devh)
            if devInfo.HasValue then
                DeltaAnalogAxis(devInfo.Value.Names.Product+".deltaX", dx) |> ignore
                DeltaAnalogAxis(devInfo.Value.Names.Product+".deltaY", dx) |> ignore
                [0..4]
                |> Seq.iter (fun (buttonNum:int) ->
                        let bitVal:UInt32 = uint32 1<<<(buttonNum*2)
                        if  (bitVal &&& buttons) = bitVal then
                            SetDigitalAxis(devInfo.Value.Names.Product+".button"+
                                           buttonNum.ToString(),true) |> ignore
                        else
                            SetDigitalAxis(devInfo.Value.Names.Product+".button"+
                                           buttonNum.ToString(),false) |> ignore
                    )
                if (buttons &&& 0x0400ul ) = 0x0400ul then
                    DeltaAnalogAxis(devInfo.Value.Names.Product+".deltaWheel",
                                    dWheel) |> ignore
            else
                ()
                
       let uint32ToHidUsage (num:uint32):HIDDesktopUsages =
           let usage:HIDDesktopUsages =  LanguagePrimitives.EnumOfValue num
           usage
          
       let doButtonDownEvent (devh:HANDLE) (usageBase:UInt32) (values:bool[]) =
            let devInfo:Nullable<DeviceInfo> = NativeAPI.GetDeviceInfo(devh)
            if devInfo.HasValue then
                [0..values.Length]
                |> Seq.iter (fun (index:int) ->
                        let usage:HIDDesktopUsages =
                            uint32ToHidUsage (usageBase + uint32 index) 
                        let name = devInfo.Value.Names.Product + "." + usage.ToString()
                        SetDigitalAxis(name,values[index]) |> ignore
                    )
                
            else
                ()
                
       let doAxisChangeEvent(devh:HANDLE) (usages:uint32[]) (values:uint32[]) =
            let devInfo:Nullable<DeviceInfo> = NativeAPI.GetDeviceInfo(devh)
            if devInfo.HasValue then
                [0..usages.Length]
                |> Seq.iter (fun index ->
                        let hidUsage:HIDDesktopUsages =
                            LanguagePrimitives.EnumOfValue usages[index]
                        let name = devInfo.Value.Names.Product+"."+
                                   hidUsage.ToString()
                        SetAnalogAxis(name,float values[index]) |> ignore       
                    )
       let messagePump():unit =
           NativeAPI.OpenWindow()
           |> fun wrapper ->
               rawInput <- Some(RawInput(wrapper))
               rawInput.Value.add_KeyStateChangeEvent (
                    Action<HANDLE,uint16, KeyState>(doKbEvent))
               rawInput.Value.add_MouseStateChangeEvent(
                   Action<HANDLE,int,int,UInt32,int>(doMouseEvent))
               rawInput.Value.add_ButtonDownEvent(
                   Action<HANDLE, UInt32, bool[]>(doButtonDownEvent))
               rawInput.Value.add_AxisEvent(
                   Action<HANDLE,uint32[], uint32[]>(doAxisChangeEvent))
               NativeAPI.MessagePump(wrapper)
       let messagePumpThread =
           Thread(ThreadStart(messagePump))
 
       do messagePumpThread.Start()
       
       member val RawInput = rawInput with get
       member val PumpThread = messagePumpThread with get
       
       interface InputDeviceInterface with
           member this.Controllers =
               NativeAPI.RefreshDeviceInfo()
               NativeAPI.GetDevices()
               |> Array.fold(fun state (devInfo:DeviceInfo) ->
                        Console.WriteLine(devInfo.Names.Product+":"+
                                          devInfo.DeviceCaps.Usage.ToString())
                        let usage:HIDDesktopUsages =
                            Microsoft.FSharp.Core.LanguagePrimitives.
                                EnumOfValue<uint, HIDDesktopUsages>(
                                    ((uint devInfo.DeviceCaps.UsagePage)<<<16)|||
                                     uint devInfo.DeviceCaps.Usage)
                        match usage  with
                        | HIDDesktopUsages.GenericDesktopMouse ->
                            MouseNode(devInfo):>Node :: state
                        | HIDDesktopUsages.GenericDesktopKeyboard ->
                            KeyboardNode(devInfo):>Node :: state
                        | HIDDesktopUsages.GenericDesktopJoystick ->
                            JoystickNode(devInfo):>Node :: state
                        | _ -> state
                   ) List.Empty
          
           member val StateChanges = (Map.empty, Map.empty, Map.empty)
            
           
