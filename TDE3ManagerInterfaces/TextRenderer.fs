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
    abstract member FontList : string list with get
    abstract member LoadFont : string -> Font
    abstract member RenderText: Vector2->Text->unit

