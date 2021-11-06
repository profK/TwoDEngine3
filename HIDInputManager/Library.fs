module HIDInputManager


open System
open HidLibrary
open HidLibrary.HIDUsages
open ManagerRegistry
open TwoDEngine3.ManagerInterfaces.InputManager

type UsageNode (usageId:byte, parent:Node option) =  
    inherit Node(Enum.ToObject(typeof(Desktop),usageId).ToString(),
                 
                 ,parent)
    


[<Manager("A cross platform HID input manager", supportedSystems.Windows)>]
type HIDInputManager() =
    let devices =
        HidDevices.Enumerate()
        |> Seq.fold ( fun dlist dev ->
            match dev.Capabilities.Usage with
            | 4 ->
                dev.Buttons.buttons
                |> Seq.fold (fun blist (button:HidButton) ->
                        button.Usages
                        |> Seq.fold(fun ulist usage ->
                            match usage with
                            | num when (num>=0x30)&&(num <= 0x93) ->
                                UsageNode(usage)::ulist
                            | _ -> ulist
                            ) List.Empty
                        |> fun ulist ->
                            match ulist with
                            | [] -> blist
                            | _ -> ButtonNode(ulist)::blist
                    ) List.Empty
                |> fun blist ->
                    match blist with
                    | [] -> dlist 
                    | _ -> DeviceNode(blist)::dlist
            | _ -> dlist
            ) List.Empty
