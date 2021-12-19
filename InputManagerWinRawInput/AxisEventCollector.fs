module AxisEventCollector

open System.Collections.Concurrent
open System.Collections.Generic
open RawInputLight
open TDE3ManagerInterfaces.InputDevices

type AxisEventCollector()=
       let axisStateCollector:Dictionary<string,AxisEvent> =
           Dictionary<string,AxisEvent>()
       member this.DeltaAnalogAxis (name:string, value:float):AxisEvent =
           lock axisStateCollector (fun () ->
               if (axisStateCollector.ContainsKey(name)) then
                   let (DeltaEvents currentValue) = axisStateCollector[name]
                   axisStateCollector[name] =
                       DeltaEvents(value+currentValue) |> ignore
                else
                    axisStateCollector.Add(
                       name, DeltaEvents(value))
           )
           axisStateCollector[name]
          
                    
       member this.SetAnalogAxis (name:string, value:float):AxisEvent =
           lock axisStateCollector (fun () ->
               if (axisStateCollector.ContainsKey(name)) then
                   let (AnalogEvents currentValue) = axisStateCollector[name]
                   axisStateCollector[name] =
                       AnalogEvents(value) |> ignore
                else
                    axisStateCollector.Add(
                       name, AnalogEvents(value))
           )
           axisStateCollector[name]
           
       member this.SetDigitalAxis(name:string, value:bool):AxisEvent =
           lock axisStateCollector (fun () ->
               if (axisStateCollector.ContainsKey(name)) then
                   let (DigitalEvents (down, up)) = axisStateCollector[name]
                   axisStateCollector[name] =
                       DigitalEvents((down || value), (up && value)) |> ignore
                else
                    axisStateCollector.Add(
                       name, DigitalEvents((not value),value))
           )
           axisStateCollector[name]
       member this.SetKeyboardAxis(name:string,key:char, keystate:KeyState):AxisEvent =
         lock axisStateCollector (fun () ->
           if (axisStateCollector.ContainsKey(name)) then
               let (KeyboardEvents (downKeys, upKeys)) = axisStateCollector[name]
               axisStateCollector[name] =
                   match keystate with
                   | KeyState.KeyDown ->
                           KeyboardEvents((key::downKeys,upKeys))
                   | KeyState.KeyUp ->
                           KeyboardEvents((downKeys,key::upKeys))
               |> ignore     
            else
                axisStateCollector.Add(
                   name, KeyboardEvents(
                       match keystate with
                       | KeyState.KeyDown ->
                               ([key],[])
                       | KeyState.KeyUp ->
                               ([],[key])
                   )
                )
         )      
         axisStateCollector[name]
       member this.Reset():Map<string,AxisEvent> = 
           lock axisStateCollector (fun () ->
               let result =
                   axisStateCollector
                   |> Seq.fold (fun (map:Map<string,AxisEvent>) kvp ->
                        map.Add(kvp.Key,kvp.Value)
                       ) Map.empty
               axisStateCollector.Clear()
               result
           )
               