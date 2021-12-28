namespace FSTree

module Tree =

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