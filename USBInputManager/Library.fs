namespace USBInputManager


open LibUsbDotNet.Info
open LibUsbDotNet.LibUsb
open ManagerRegistry
open TwoDEngine3.ManagerInterfaces.InputManager

[<Manager("USB based input manager",supportedSystems.Windows)>]
type USBInputManager() =
    let context = new UsbContext()
    let axes = context.List() |>
                Seq.fold (
                    fun state (usbDevice:IUsbDevice) ->
                        match usbDevice.TryOpen() with
                        | true ->
                            usbDevice.Configs |> Seq.fold (
                                fun state (configInfo:UsbConfigInfo) ->
                                    configInfo.Interfaces |> Seq.fold(
                                        fun state (interfaceInfo:UsbInterfaceInfo) ->
                                            interfaceInfo.Endpoints |> Seq.fold (
                                                fun state (endPoint:UsbEndpointInfo) ->
                                                    //TODO build device desc tree
                                                ) state
                                        ) state
                                ) state
                        | false -> state
                    ) List.Empty
    
    member val AxisTest = axes with get
    
    interface InputManager with
        
        member this.PollAllAxes() = failwith "todo"
        member this.PollChangedAxes() = failwith "todo"