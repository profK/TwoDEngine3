namespace FSTree

module Tree =


type TreeNode<'T when 'T:equality>(?data) =
    member val Data:'T option = defaultArg data None
    member val Children = List.Empty with get, set
   // builderInterface
    member this.AddChild childNode : TreeNode<'T> =
        this.Children<-childNode::this.Children
        this 
    member this.RemoveChild childNode : TreeNode<'T>=
        this.Children <-
            this.Children
            |> List.except [childNode]
        this
    member this.FindChild<'T> data : TreeNode<'T> option=
        this.Children
        |> List.tryFind (fun (node:TreeNode<'T>) -> node.Data = data)
        
// a tree root is just a tree node         
type Tree<'T when 'T:equality>() =
    inherit TreeNode<'T>(None)
    
let Empty<'T when 'T:equality> =
    Tree<'T>()
    
type IterFunc<'T when 'T:equality>= TreeNode<'T>->TreeNode<'T>->unit
let rec iterNode<'T when 'T:equality>
    (iterFunc:IterFunc<'T>) (parentNode:TreeNode<'T>) (treeNode:TreeNode<'T>)
    = 
    iterFunc parentNode treeNode
    treeNode.Children
    |>List.iter( fun child ->
        iterNode iterFunc treeNode child |> ignore)
    
let rec iter<'T when 'T:equality> (iterFunc:IterFunc<'T>) (root:Tree<'T>)=
    root.Children
    |>List.iter( fun child ->
        iterNode iterFunc child |> ignore)
   
//TODO; Mapping Untested!     
type MapFunc<'T when 'T:equality>= TreeNode<'T>->TreeNode<'T>->TreeNode<'T>

let rec mapNode<'T when 'T:equality>
    (mapFunc:MapFunc<'T>) (parentNode:TreeNode<'T>) (treeNode:TreeNode<'T>):TreeNode<'T>= 
    let newNode:TreeNode<'T> = mapFunc parentNode treeNode
    newNode.Children<-
        treeNode.Children
        |> List.map( fun child ->
            newNode.AddChild (mapNode mapFunc newNode child))
    newNode
    
   
let map<'T when 'T:equality> (mapFunc:MapFunc<'T>) (tree:Tree<'T>):Tree<'T> =
    let newTree = Tree()
    newTree.Children<-
        tree.Children
        |> List.map( fun child ->
            newTree.AddChild (mapNode mapFunc newTree child))
    newTree
 
 // folding
type FoldFunc<'T, 'U when 'T:equality>=
   'U->TreeNode<'T>->TreeNode<'T>->'U
let rec foldNode<'T, 'U when 'T:equality>
    (foldFunc:FoldFunc<'T,'U>) (state:'U)(parentNode:TreeNode<'T>) (treeNode:TreeNode<'T>):'U= 
        let newState = foldFunc state parentNode treeNode
        treeNode.Children
        |> List.fold
            ( fun state child -> foldNode foldFunc newState treeNode child)
            newState
    
let fold<'T, 'U when 'T:equality> (foldFunc:FoldFunc<'T,_>) (state) (tree:Tree<'T>): 'U =
    tree.Children
    |> List.fold
        ( fun state child -> foldNode foldFunc state tree child)
        state
    
        
