module Tree

    open System
 
    type Tree<'LeafData,'INodeData> =
        | LeafNode of 'LeafData
        | InternalNode of 'INodeData * Tree<'LeafData,'INodeData> seq
    let rec iter fLeaf fNode (tree:Tree<'LeafData,'INodeData>) =
        let recurse = iter fLeaf fNode
        match tree with
        | LeafNode leafInfo ->
            fLeaf leafInfo
        | InternalNode (nodeInfo,subtrees) ->
            fNode nodeInfo
            subtrees
            |>Seq.iter recurse           
    let rec cata fLeaf fNode (tree:Tree<'LeafData,'INodeData>) :'r =
        let recurse = cata fLeaf fNode
        match tree with
        | LeafNode leafInfo ->
            fLeaf leafInfo
        | InternalNode (nodeInfo,subtrees) ->
            fNode nodeInfo (subtrees |> Seq.map recurse)
    let rec fold fLeaf fNode acc (tree:Tree<'LeafData,'INodeData>) :'r =
        let recurse = fold fLeaf fNode
        match tree with
        | LeafNode leafInfo ->
            fLeaf acc leafInfo
        | InternalNode (nodeInfo,subtrees) ->
            // determine the local accumulator at this level
            let localAccum = fNode acc nodeInfo
            // thread the local accumulator through all the subitems using Seq.fold
            let finalAccum = subtrees |> Seq.fold recurse localAccum
            // ... and return it
            finalAccum
            
    let rec foldBack fLeaf fNode acc (tree:Tree<'LeafData,'INodeData>) :'r =
        let recurse = foldBack fLeaf fNode
        match tree with
        | LeafNode leafInfo ->
            fLeaf acc leafInfo
        | InternalNode (nodeInfo,subtrees) ->
            let localAcc = (Activator.CreateInstance(acc.GetType()));
            subtrees
            |> Seq.fold recurse 
            |> fNode acc nodeInfo 
    
    let rec except (ignore:'TreeType seq) 
        (tree:Tree<'LeafData,'INodeData>) : 'r =
        let recurse = except ignore
        let treetype = typeof<Tree<'LeafData,'INodeData>>
        cata
            (fun leafData ->
                Activator.CreateInstance(treetype,LeafNode(leafData))
                :?> Tree<'LeafData,'INodeData>
            )
            (fun nodeInfo subtrees ->
                Activator.CreateInstance(treetype,
                    InternalNode(
                        nodeInfo,
                        subtrees
                        |> Seq.except ignore
                        |> Seq.map recurse)
                ) :?> Tree<'LeafData,'INodeData>
            ) tree
                    
                    
    let rec insert (parent:Tree<'LeafData,'INodeData>)
                    (node:Tree<'LeafData,'INodeData>)
                    (tree:Tree<'LeafData,'INodeData>): 'r =
        let recurse = insert parent node
        let treetype = typeof<Tree<'LeafData,'INodeData>>
        if parent<>tree then
            cata
                (fun leafData ->
                    Activator.CreateInstance(treetype,LeafNode(leafData))
                    :?> Tree<'LeafData,'INodeData>
                )
                (fun nodeInfo subtrees ->
                    Activator.CreateInstance(treetype,
                        InternalNode(
                            nodeInfo,
                            subtrees
                            |> Seq.map recurse)
                    ) :?> Tree<'LeafData,'INodeData>
                )
                tree
        else // at insert point
            cata
                (fun leafData ->
                    Activator.CreateInstance(treetype,
                        InternalNode(leafData,[node]))
                    :?> Tree<'LeafData,'INodeData>
                )
                (fun nodeInfo subtrees ->
                    Activator.CreateInstance(treetype,
                        InternalNode(
                            nodeInfo,
                            node::(subtrees
                                    |> Seq.map recurse
                                    |> Seq.toList)
                        )    
                    ) :?> Tree<'LeafData,'INodeData>
                )
                tree