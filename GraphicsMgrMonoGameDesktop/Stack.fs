module Stack

let push<'T> element (stack: 'T list) : 'T list = element :: stack

let pop<'T> (stack: 'T list) : ('T * 'T list) = (stack.Head, stack.Tail)
