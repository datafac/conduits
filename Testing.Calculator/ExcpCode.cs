namespace Testing.Calculator;

public enum ExcpCode
{
    Undefined = 0,
    // general errors
    DeserializationError = 1,
    DeadlineExceeded = 2,
    UnsupportedRequestType = 3,
    UnsupportedResponseType = 4,
    OtherException = 5,
    // calculator errors
    DivideByZero = 6,
    Overflow = 7,
}
