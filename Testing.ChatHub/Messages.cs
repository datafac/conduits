using Nerdbank.MessagePack;
using PolyType;
using System;
using System.Net;

namespace Testing.ChatHub;

public enum ErrorCode
{
    None = 0,

    // weather errors

    // general errors
    InvalidData = 97,
    InvalidOp = 98,
    OtherException = 99,

}

[GenerateShape]
public readonly partial struct MessageId : IEquatable<MessageId>, IComparable<MessageId>
{
    [Key(1)] public long Value { get; }
    public MessageId(long value) => Value = value; // todo?[ConstructorShape] 
    public override string ToString() => $"MessageId({Value})";
    public bool Equals(MessageId other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is MessageId other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Value);
    public static bool operator ==(MessageId left, MessageId right) => left.Equals(right);
    public static bool operator !=(MessageId left, MessageId right) => !left.Equals(right);
    public int CompareTo(MessageId other) => Value.CompareTo(other.Value);
    public static bool operator <(MessageId left, MessageId right) => left.CompareTo(right) < 0;
    public static bool operator >(MessageId left, MessageId right) => left.CompareTo(right) > 0;
    public static bool operator <=(MessageId left, MessageId right) => left.CompareTo(right) <= 0;
    public static bool operator >=(MessageId left, MessageId right) => left.CompareTo(right) >= 0;
}

[GenerateShape]
public readonly partial struct Address : IEquatable<Address>
{
    [Key(1)] public string Value { get; } = string.Empty;
    public Address(string value) => Value = value; // todo?[ConstructorShape] 
    public bool Equals(Address other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is Address other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Value);
    public override string ToString() => $"Address({Value})";
}

[GenerateShape]
[DerivedTypeShape(typeof(GetMessagesRequest), Tag = 1)]
public abstract partial class RequestBase { }

[GenerateShape]
public sealed partial class GetMessagesRequest : RequestBase
{
    [Key(1)] public Address Username { get; set; }
    [Key(2)] public MessageId LastMessageId { get; set; }
}

[GenerateShape]
[DerivedTypeShape(typeof(ErrorResult), Tag = 1)]
[DerivedTypeShape(typeof(Envelopes), Tag = 2)]
[DerivedTypeShape(typeof(Envelope), Tag = 3)]
public abstract partial class ResultBase { }

[GenerateShape]
public sealed partial class Envelope : ResultBase
{
    [Key(1)] public Address Source { get; set; }
    [Key(2)] public Address Target { get; set; }
    [Key(3)] public MessageId LastMessageId { get; set; }
    [Key(4)] public string? Content { get; set; }
}

[GenerateShape]
public sealed partial class Envelopes : ResultBase
{
    [Key(1)] public Envelope[] Batch { get; set; } = Array.Empty<Envelope>();
}

[GenerateShape]
public sealed partial class ErrorResult : ResultBase
{
    [Key(1)] public ErrorCode Code { get; set; }

    [Key(2)] public string Message { get; set; } = string.Empty;
}

