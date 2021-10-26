module Stack

let push<'T> element (stack: 'T list) = element :: stack

let pop<'T> (stack: 'T list) = (stack.Head, stack.Tail)
