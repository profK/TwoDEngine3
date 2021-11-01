module HIDInputManager

open System.Reflection.Metadata
open DevDecoder.HIDDevices
open DynamicData
open ManagerRegistry
open TwoDEngine3.ManagerInterfaces.InputManager


type HIDAxisNode(name, value,parent) =
    inherit Node(name, value,parent)

type HIDControlNode(control: Control,parent:Node) as this =
    inherit Node(control.Name,
                 Children(
                     { 0 .. (control.ElementCount - 1) }
                     |> Seq.map
                         (fun index ->
                             HIDAxisNode(
                                 index.ToString(),
                                 match control.IsBoolean with
                                 | true -> Axis(Digital(false))
                                 | false -> Axis(Analog(float 0))
                                 , Some(this :> Node)
                             )
                             :> Node)
                     |> Seq.toList
                 ) ,Some(parent) )

type HIDDeviceNode(device: Device) as this =
    inherit Node(device.Name,
                 Children(
                     device.Controls
                     |> Seq.map (fun ctrl -> (HIDControlNode(ctrl,this) :> Node))
                     |> Seq.toList
                 ), None)

    member val Device = device
    

    member val listenTo = false with get, set

type ControlChange(devNode,ctrlNode,oldVal,newVal,tstamp) =
        member val DeviceNode = devNode with get
        member val ControlNode = ctrlNode with get
        member val OldValue = oldVal with get
        member val NewValue = newVal with get
        member val Timestamp = tstamp with get

[<Manager("A cross platform HID input manager", supportedSystems.Windows)>]
type HIDInputManager() =
    let root = Node("root", Children(List.Empty), None)
    let devices = new Devices()

    let hidDevices =
        devices
            .Connected()
            .Subscribe(fun changeSet ->
                changeSet
                |> Seq.iter
                    (fun action ->
                        match action.Reason with
                        | ChangeReason.Add ->
                            root.Value <-
                                Children(
                                    (HIDDeviceNode(action.Current) :> Node)
                                    :: match root.Value with
                                       | Children oldlist -> oldlist
                                       | _ -> List.Empty
                                )

                            printfn $"Added %s{action.Current.Name}"
                        | ChangeReason.Remove ->
                            root.Value <-
                                Children(
                                    match root.Value with
                                    | Children oldlist ->
                                        oldlist
                                        |> List.filter (fun childNode -> childNode.Name = action.Current.Name)
                                    | _ -> List.empty
                                )

                            printfn $"Removed %s{action.Key}"
                        | _ -> printfn $"Unimplemented action: %s{action.Reason.ToString()}"))
    
        
    let mutable changeList:ControlChange list = List.empty
    
    let QueueCtrlChange(devNode, ctrlNode, oldVal, newVal, tstamp) =
        changeList <- (ControlChange(devNode,ctrlNode,oldVal,newVal,tstamp) :: changeList)
            
    interface InputManager with
        member this.ListenTo(node:Node) =
            match node with
            | :? HIDDeviceNode ->
                (node :?> HIDDeviceNode).Device.Subscribe(fun (controlChanges) ->
                    controlChanges
                    |> Seq.iter ( fun controlChange ->
                        QueueCtrlChange(node,controlChange.Control,controlChange.PreviousValue,
                                        controlChange.Value,controlChange.Timestamp
                                        )
                        )
                ) |> ignore
            | :? HIDControlNode -> (this:>InputManager).ListenTo(node.Parent.Value)
            | :? HIDAxisNode -> (this:>InputManager).ListenTo(node.Parent.Value)
        member this.StateChanges = failwith "todo"
        member val controllerRoot = root
