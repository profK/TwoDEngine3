module AxisStateCollector

open System.Collections.Concurrent
open System.Collections.Generic
open RawInputLight
open TDE3ManagerInterfaces.InputDevices

type AxisStateCollector()=
       let axisStateCollector:Dictionary<string,AxisState> =
           Dictionary<string,AxisState>()
       member this.DeltaAnalogAxis (name:string, value:float):AxisState =
           lock axisStateCollector (fun () ->
               if (axisStateCollector.ContainsKey(name)) then
                   let (DeltaState currentValue) = axisStateCollector[name]
                   axisStateCollector[name] =
                       DeltaState(value+currentValue) |> ignore
                else
                    axisStateCollector.Add(
                       name, DeltaState(value))
           )
           axisStateCollector[name]
          
                    
       member this.SetAnalogAxis (name:string, value:float):AxisState =
           lock axisStateCollector (fun () ->
               if (axisStateCollector.ContainsKey(name)) then
                   let (AnalogState currentValue) = axisStateCollector[name]
                   axisStateCollector[name] =
                       AnalogState(value) |> ignore
                else
                    axisStateCollector.Add(
                       name, AnalogState(value))
           )
           axisStateCollector[name]
           
       member this.SetDigitalAxis(name:string, value:bool):AxisState =
           lock axisStateCollector (fun () ->
               if (axisStateCollector.ContainsKey(name)) then
                   let (DigitalState value) = axisStateCollector[name]
                   axisStateCollector[name] =
                       DigitalState value
                   |> ignore
                else
                    axisStateCollector.Add(
                       name, DigitalState value )
           )
           axisStateCollector[name]
       member this.SetKeyboardAxis(name:string,key:char, keystate:KeyState):AxisState =
         lock axisStateCollector (fun () ->
           if (axisStateCollector.ContainsKey(name)) then
               let (KeyboardState downKeys) = axisStateCollector[name]
               axisStateCollector[name] =
                   match keystate with
                   | KeyState.KeyDown ->
                           KeyboardState (key::downKeys)
                   | KeyState.KeyUp ->
                           KeyboardState(
                               downKeys
                               |> List.except [key])
               |> ignore     
            else
                axisStateCollector.Add(
                   name, KeyboardState(
                       match keystate with
                       | KeyState.KeyDown ->
                               [key]
                       | KeyState.KeyUp ->
                               [] // this really shouldnt happen
                   )
                )
         )      
         axisStateCollector[name]
       member this.GetState():Map<string,AxisState> = 
           lock axisStateCollector (fun () ->
               let result =
                   axisStateCollector
                   |> Seq.fold (fun (map:Map<string,AxisState>) kvp ->
                        map.Add(kvp.Key,kvp.Value)
                       ) Map.empty
               result
           )
               