module TDE3ManagerInterfaces.TextRendererInterfaces

open TwoDEngine3.ManagerInterfaces.GraphicsManagerInterface
type Font =
    abstract member Name : string
    abstract member Size : int
    abstract member MakeText : string -> Text

and Text =
    abstract member Text : string
    abstract member Font : Font

and TextManager =
    abstract member FontList : string list
    abstract member LoadFont : string -> Font
    abstract member RenderText : Text  -> unit
