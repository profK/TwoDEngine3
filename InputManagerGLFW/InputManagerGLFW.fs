namespace InputManagerGLFW

open TDE3ManagerInterfaces.InputDevices
open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
open TwoDEngine3.ManagerInterfaces.InputManager
open GLFW

type InputManagerGLFW() as this =
   let graphics =
        ManagerRegistry
            .getManager<GraphicsManager>()
                .Value
                
   let window = Glfw.CurrentContext
   let mutable currentKeysDown = List.Empty
   do Glfw.SetKeyCallback(window,fun window key scancode action mods ->
            printfn($"Scancode ${scancode}")
       ) |> ignore
   do graphics.GraphicsListeners <- (this:>GraphicsListener) :: graphics.GraphicsListeners
   let mutable lastTree:Node list = List.empty 
   let mutable currentTree:Node list = List.empty
    
   interface GraphicsListener with
       member this.Render(graphics) =
           ()
       member this.Update(deltaTime) =
           Glfw.PollEvents()
           lastTree <-currentTree
           currentTree <-
               Joystick.GetValues()
               |> Array.fold (fun joylist (joystick:Joystick) ->
                    if Glfw.JoystickPresent(joystick) then
                        joystick::joylist
                    else
                        joylist
                   ) List.Empty
               |> List.map (fun joy ->
                    GLFWJoyNode(joy,None) :> Node
                   )
               |> List.append [GLFWMouseNode(None);GLFWKeyboardNode(None)]
           None // no errors
       
                    
   interface InputDeviceInterface with
       member val Controllers = currentTree with get 
       member this.StateChanges =
           let rec TreeToMap (tree:Node list) (map:Map<string,Node>) =
               tree
               |> List.fold (fun (state:Map<string,Node>) (node) ->
                     match node.Value with
                     | Children kids -> (TreeToMap kids state).Add (node.Path, node)
                     | Axis axisValue -> state.Add(node.Path,node)
                   ) map
           let subtractMaps (map1:Map<string,Node>) (map2:Map<string,Node>) =
               Map.fold (fun acc key value ->
                   if Map.containsKey key map2 then
                       Map.remove key acc
                   else
                       acc) map1 map2
           let intersectMaps (map1) (map2)=
                Map.fold (fun acc key value ->
                    if not (Map.containsKey key map2) then
                        Map.remove key acc
                    else acc) map1 map2
               
           let lastMap = TreeToMap lastTree Map.empty
           let currentMap = TreeToMap currentTree Map.empty
           let addedNodes = subtractMaps currentMap lastMap
           let removedNodes = subtractMaps lastMap currentMap
           let changedNodes =
               Map.fold (fun state (key:string) (value:Node)->
                      if  Map.containsKey key lastMap &&
                          value.Value = lastMap.[key].Value then
                          Map.remove key state
                      else
                          state
                   ) currentMap lastMap
           (addedNodes,removedNodes,changedNodes)
           
     