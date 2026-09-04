namespace Testing.Calculator;

public enum ErrorCode
{
    None = 0,

    // calculator errors
    DivideByZero = 1,
    Overflow = 2,

    // general errors
    DeserializationError = 97,
    UnsupportedRequestType = 98,
    OtherException = 99,

}
