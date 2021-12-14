module TDE3ManagerInterfaces.MouseAndKeyboardManager

open TDE3ManagerInterfaces.InputDevices

type MouseAndKeyboardManager =
    inherit InputDeviceInterface
        abstract Controllers : Node list
        // gets changes since last update
        // The first map is added nodes
        // The second map is removed nodes
        // The third map is nodes whose value has changed
        abstract StateChanges : (Map<string,Node> * Map<string,Node> * Map<string,Node>)