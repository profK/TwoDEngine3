module InputManagerWinRawInput.AxisStateCollector

open System.Collections.Concurrent
open RawInputLight
open TDE3ManagerInterfaces.InputDevices

type AxisStateCollector()=
       let axisStateCollector:ConcurrentDictionary<string,AxisUnion> =
           ConcurrentDictionary<string,AxisUnion>()
       member this.DeltaAnalogAxis (name:string, value:float):AxisUnion =
       axisStateCollector.AddOrUpdate(
               name,
                Analog(value),
                (fun (name:string) (currentAxis:AxisUnion)->
                    let (Analog currentVal) = currentAxis
                    Analog(currentVal+value)
                )
       )
                    
       member this.SetAnalogAxis (name:string, value:float):AxisUnion =
           axisStateCollector.AddOrUpdate(
                   name,
                    Analog(value),
                    (fun (name:string) (currentAxis:AxisUnion)->
                        Analog(value)
                    )
                )
           
       member this.SetDigitalAxis(name:string, value:bool) =
           axisStateCollector.AddOrUpdate(
                   name,
                    Digital(value),
                    (fun (name:string) (currentAxis:AxisUnion)->
                        let (Digital currentVal) = currentAxis
                        Digital(value)
                    )
                )
       member this.SetKeyboardAxis(name:string,key:char, keystate:KeyState) =
            match keystate with
                | KeyState.KeyDown ->
                    axisStateCollector.AddOrUpdate(
                        name,
                        Keyboard([key]),
                        (fun (name:string) (oldAxisUnion:AxisUnion)->
                            let (Keyboard oldList) = oldAxisUnion
                            Keyboard(
                                (key)::oldList
                            )
                        )
                    ) |> ignore
                | KeyState.KeyUp ->
                    axisStateCollector.AddOrUpdate(
                        name,
                        Keyboard([]),
                        (fun (name:string) (oldAxisUnion:AxisUnion) ->
                            let (Keyboard oldList) = oldAxisUnion 
                            Keyboard(
                                    oldList
                                    |> List.except([key])
                                )
                            )
                        )|> ignore
       member this.GetState() =
           axisStateCollector
           |> Seq.fold (fun (map:Map<string,AxisUnion>) kvp ->
                map.Add(kvp.Key,kvp.Value)
               ) Map.empty