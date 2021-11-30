module TwoDEngine3.ManagerInterfaces.InputManager

open System.Collections.Specialized


type AxisUnion =
    | Digital of bool
    | Analog of float
    | Hat of int
    | Keyboard of char list

and NodeValue =
    | Axis of AxisUnion
    | Children of Node list

and Node =
    abstract Name: string with get
    abstract Value: NodeValue with get
    abstract Parent: Node option with get

type ChangeType=
    | NotChanged
    | Added
    | Removed
    | ValueChanged
    
type InputManager =
    abstract Controllers : Node list
    abstract StateChanges : (Node * ChangeType) seq
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
    
