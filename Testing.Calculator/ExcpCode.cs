namespace Testing.Calculator;

public enum ExcpCode
{
    Undefined = 0,
    // general errors
    DeserializationError = 1,
    DeadlineExceededqqq = 2,
    OperationCancelled = 3,
    UnsupportedRequestType = 4,
    UnsupportedResponseType = 5,
    OtherException = 6,
    // calculator errors
    DivideByZero = 7,
    Overflow = 8,
}
