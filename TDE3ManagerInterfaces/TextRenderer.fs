module TDE3ManagerInterfaces.TextRendererInterfaces

open System.Numerics

type Font =
    abstract member Name : string
    abstract member Size : int
    abstract member MakeText : string -> Text
    
and Text =
    abstract member Text : string
    abstract member Font : Font
       
and TextManager =
    abstract member FontList : Font list with get
    abstract member RenderText: Vector2->Text->unit

