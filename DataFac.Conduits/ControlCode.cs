namespace DataFac.Conduits;

public enum ControlCode
{
    None = 0, // user payload
    Timeout = 1, // deadline exceeded
    Cancelled = 2, // operation cancelled
}
