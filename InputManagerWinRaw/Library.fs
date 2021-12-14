namespace InputManagerWinRaw

open System
open System.Threading
open RawInputLight
open TDE3ManagerInterfaces.InputDevices


type InputManagerWinRaw() as this=
    [<STAThread>]
    let inputThread() =
        NativeAPI.OpenWindow()
        |> fun (window:NativeAPI.HWND_WRAPPER) ->
            this.RawInputOpt <- Some(RawInput(window))
            NativeAPI.MessagePump(window)
        
    let thread  = Thread(ThreadStart(inputThread)).Start()
    
    member val RawInputOpt:RawInput option = None with get,set   
        
    interface InputDeviceInterface with
        
        member val Controllers =
            let stringCompare (str1:string, str2:string) =
                if (str1.Length<str2.Length) then
                    -1
                else if (str1.Length>str2.Length) then
                    +1
                else
                    [0..str1.Length-1]
                    |> List.tryFindIndex(fun idx ->
                        not (str1.Chars(idx) = str2.Chars(idx))
                        )
                    |> function
                        | None -> 0
                        | Some idx ->
                            let char1 = str1.Chars(idx)
                            let char2 = str2.Chars(idx)
                            if (char1<char2) then
                                -1
                            else if (char2>char1) then
                                 1
                            else
                                 0
                                
            NativeAPI.GetDevices()
            |> Seq.sortWith(fun dev1 dev2 ->
                    stringCompare(dev1.Names.Product, dev2.Names.Product)  
                )
               
            |> Seq.map
        member val StateChanges = failwith "todo"
        
        