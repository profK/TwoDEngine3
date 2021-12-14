module TwoDEngine3.ManagerInterfaces.InputManager

open System.Collections.Specialized
open TDE3ManagerInterfaces.InputDevices

type GameControllerManager =
    inherit InputDeviceInterface 
        abstract Controllers : Node list
        // gets changes since last update
        // The first map is added nodes
        // The second map is removed nodes
        // The third map is nodes whose value has changed
        abstract StateChanges : (Map<string,Node> * Map<string,Node> * Map<string,Node>)
let FlattenTree tree:Node list =
    let rec Recursor nodes:Node list =
        nodes |> List.fold (
            (fun (nodeList:Node list) (node:Node) ->
                (match node.Value with
                | Children childList -> node::(Recursor childList)
                | Axis axisValue-> [node]
                )@ nodeList
            ) 
        ) List.Empty
    Recursor(tree)
    
let ListDifference (list1:Node list) (list2:Node list)  =
    list1 |> List.fold (fun diff node ->
            list2 |>List.tryFind (fun list2node ->
                node.Name = list2node.Name)
            |> function
               | Some oldNode -> node::diff
               | None -> diff
        ) List.Empty
    
let GetTreeChanges (tree1:Node list) (tree2:Node list) =
    let flatTree1 = FlattenTree tree1
    let flatTree2 = FlattenTree tree2
    let addedNodes =ListDifference flatTree1 flatTree2
    let removedNodes = ListDifference flatTree2 flatTree1
    let continuingNodes = ListDifference flatTree1 addedNodes
    (addedNodes,removedNodes,continuingNodes)
    
