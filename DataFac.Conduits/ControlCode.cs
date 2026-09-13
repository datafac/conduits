namespace DataFac.Conduits;

public enum ControlCode
{
    None = 0, // user payload
    GetAppInfo = 1, // get app info

    // errors
    Timeout = 95, // deadline exceeded
    Cancelled = 96, // operation cancelled
    InvalidData = 97, // invalid response data
    InvalidOp = 98, // invalid operation for this conduit
    Exception = 99, // other error
}
