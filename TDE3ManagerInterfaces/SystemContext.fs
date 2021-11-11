module TDE3ManagerInterfaces.SystemContext

type SystemContext =
    // currently just a marker
    abstract member SetGlobal: name:string->obj->obj
    abstract member GetGlobal: name:string ->obj