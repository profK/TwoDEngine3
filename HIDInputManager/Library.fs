namespace HIDInputManager

open System.Reflection.Metadata
open DevDecoder.HIDDevices
open DynamicData
open ManagerRegistry
open TwoDEngine3.ManagerInterfaces.InputManager



[<Manager("A cross platform HID input manager", supportedSystems.Windows)>]
type HIDInputManager() =


    let rec GetControlNodes (controls: Control seq) : Node list =
        controls
        |> Seq.map
            (fun (control: Control) ->
                Node(
                    control.Name,
                    Children(
                        { 0 .. (control.ElementCount-1) }
                        |> Seq.map (fun index ->
                            Node(index.ToString(),
                                 match control.IsBoolean with
                                 | true -> Axis(Digital(false))
                                 | false-> Axis(Analog(float 0))               
                                )
                            )
                        |> Seq.toList
                        )      
                    )
                )
            |> Seq.toList
         

    let root = Node("root", Children(List.Empty))

    let devices = new Devices()

    let hidDevices =
        devices
            .Connect()
            .Subscribe(fun changeSet ->
                changeSet
                |> Seq.iter
                    (fun action ->
                        match action.Reason with
                        | ChangeReason.Add ->
                            root.Value <-
                                Children(
                                    Node(action.Current.Name, Children(GetControlNodes action.Current.Controls))
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

    interface InputManager with
        member val controllerRoot = root
