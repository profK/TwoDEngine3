namespace HIDInputManager

open DevDecoder.HIDDevices
open DynamicData
open ManagerRegistry
open TwoDEngine3.ManagerInterfaces.InputManager



[<Manager("A cross platform HID input manager", supportedSystems.Windows)>]
type HIDInputManager() =


    let rec GetControlNodes (controls: Control seq) : Node seq =
        controls
        |> Seq.map
            (fun (control: Control) ->
                if control.IsBoolean then
                    Node(control.Name, Axis(Digital(false)))
                else
                    Node(control.Name, Axis(Analog(float 0))))

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
                                    Node(action.Key, Children(List.Empty))
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
                                        |> List.filter (fun childNode ->
                                            childNode.Name = action.Current.Name)
                                    | _ -> List.empty
                                )

                            printfn $"Removed %s{action.Key}"
                        | _ -> printfn $"Unimplemented action: %s{action.Reason.ToString()}"
                    )
                )

    interface InputManager with
        member val controllerRoot = root
