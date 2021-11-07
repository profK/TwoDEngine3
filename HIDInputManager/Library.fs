module HIDInputManager


open System
open HidLibrary
open ManagerRegistry
open TwoDEngine3.ManagerInterfaces.InputManager

type UsageNode(num:uint16,value,parent) =
    inherit Node(Enum.ToObject(typeof<HIDUsages.Desktop>,num).ToString(),
                 value,parent)
    

type DeviceNode(name,usages: uint16 list) as this=
    inherit Node(name,
                 Children(
                     usages
                     |> List.map (fun usage->
                            UsageNode(usage, Axis(Digital(false)),
                                      Some(this :> Node)) :> Node
                         )
                     )
                 ,None)
    


[<Manager("A cross platform HID input manager", supportedSystems.Windows)>]
type HIDInputManager() =
    let devices =
        HidDevices.Enumerate()
        |> Seq.fold ( fun dlist dev ->
            match dev.Capabilities.Usage with
            | 4s ->
                dev.Buttons.buttons
                |> Seq.fold (fun ulist (button:HidButton) ->
                        button.Usages
                        |> Seq.fold(fun ulist usage ->
                            match usage with
                            | num when ((num>=uint16 0x30)&&(num <=uint16 0x93)) ->
                                usage::ulist
                            | _ -> ulist
                            ) ulist
                    ) List.Empty
                |> fun usageList ->
                        match usageList with
                        | [] -> dlist
                        | _ -> DeviceNode(dev.Name,usageList)::dlist
            | _ -> dlist
            ) List.Empty
