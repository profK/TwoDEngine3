module Stack

let push<'T> element (stack: 'T list) : 'T list = element :: stack

let pop<'T> (stack: 'T list) : ('T option * 'T list) = (stack |> List.tryHead, stack.Tail)
