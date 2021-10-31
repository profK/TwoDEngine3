namespace USBInputManager


open LibUsbDotNet.Info
open LibUsbDotNet.LibUsb
open ManagerRegistry
open TwoDEngine3.ManagerInterfaces.InputManager

[<Manager("USB based input manager",supportedSystems.Windows)>]
type USBInputManager() =
    let context = new UsbContext()
    let axes =  Node("controllers",Children(
                    (context.List() |> Seq.filter (fun usbDevice -> usbDevice.TryOpen()))
                    |>Seq.map (fun (usbDevice:IUsbDevice) ->
                       usbDevice.
                       
    let 
    
    interface InputManager with
        member val controllerRoot = axes
        
        