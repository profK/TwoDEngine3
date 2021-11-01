module HIDInputManager

open System
open System.Reflection.Metadata
open DevDecoder.HIDDevices
open DevDecoder.HIDDevices.Controllers
open DynamicData
open ManagerRegistry
open TwoDEngine3.ManagerInterfaces.InputManager


type AxisNode(ctrl:ControlValue,parent) =
    inherit Node(ctrl.PropertyName,
                 if ctrl.Type = typeof<bool> then
                    Axis(Digital(false))
                 else
                    Axis(Analog(float 0))
                 ,Some(parent))



type ControllerNode(controller: Controller,parent) as this =
    inherit Node(controller.Name,
                 Children(
                     controller
                     |> Seq.cast<ControlValue>
                     |> Seq.map (fun ctrl -> (AxisNode(ctrl,this) :> Node))
                     |> Seq.toList
                 ), parent)


type ControlChange(devNode,ctrlNode,oldVal,newVal,tstamp) =
        member val DeviceNode = devNode with get
        member val ControlNode = ctrlNode with get
        member val OldValue = oldVal with get
        member val NewValue = newVal with get
        member val Timestamp = tstamp with get

[<Manager("A cross platform HID input manager", supportedSystems.Windows)>]
type HIDInputManager() =
    let root = Node("root", Children(List.Empty), None)
    let subsciption = (new Devices()).Controllers<Controller>().Subscribe(fun controller ->
            root.Value <- Children(
                match root.Value with
                | Children list ->
                    (ControllerNode(controller,Some(root)):>Node)::list
                | _ -> failwith "Error: root does not have a children list as its value!"
            )
        )
    
 

    
    
        
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
