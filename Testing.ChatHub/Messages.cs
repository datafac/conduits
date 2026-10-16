using Nerdbank.MessagePack;
using PolyType;
using System;
using System.ComponentModel;

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

/// <summary>
/// Sender defined message identifier.
/// </summary>
[GenerateShape]
public readonly partial struct MessageId : IEquatable<MessageId>
{
    [Key(1)] public Guid Value { get; }
    public MessageId(Guid value) => Value = value; // todo?[ConstructorShape] 
    public override string ToString() => $"MessageId({Value})";
    public bool Equals(MessageId other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is MessageId other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Value);
    public static bool operator ==(MessageId left, MessageId right) => left.Equals(right);
    public static bool operator !=(MessageId left, MessageId right) => !left.Equals(right);
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
[DerivedTypeShape(typeof(Envelope), Tag = 1)]
public abstract partial class OtherBase { }

[GenerateShape]
[DerivedTypeShape(typeof(GetMessagesQuery), Tag = 1)]
[DerivedTypeShape(typeof(SendMessageCmd), Tag = 2)]
[DerivedTypeShape(typeof(AckMessageCmd), Tag = 3)]
public abstract partial class QueryBase { }

[GenerateShape]
[DerivedTypeShape(typeof(ErrorReply), Tag = 1)]
[DerivedTypeShape(typeof(BoolReply), Tag = 2)]
[DerivedTypeShape(typeof(GetMessagesReply), Tag = 3)]
public abstract partial class ReplyBase { }

[GenerateShape]
public sealed partial class GetMessagesQuery : QueryBase
{
    [Key(1)] public Address Target { get; set; }
}

[GenerateShape]
public sealed partial class GetMessagesReply : ReplyBase
{
    [Key(1)] public Envelope[] Envelopes { get; set; } = Array.Empty<Envelope>();
}

[GenerateShape]
public sealed partial class SendMessageCmd : QueryBase
{
    [Key(1)] public MessageId MessageId { get; set; }
    [Key(2)] public Address Source { get; set; }
    [Key(3)] public Address Target { get; set; }
    [Key(4)] public string? Content { get; set; }
}

//AckMessageCmd

[GenerateShape]
public sealed partial class AckMessageCmd : QueryBase
{
    [Key(1)] public Address Target { get; set; }
    [Key(2)] public MessageId MessageId { get; set; }
}

[GenerateShape]
public sealed partial class Envelope : OtherBase
{
    [Key(1)] public MessageId MessageId { get; set; }
    [Key(2)] public Address Source { get; set; }
    [Key(3)] public Address Target { get; set; }
    [Key(4)] public string? Content { get; set; }
}

[GenerateShape]
public sealed partial class ErrorReply : ReplyBase
{
    [Key(1)] public ErrorCode Code { get; set; }

    [DefaultValue("")]
    [Key(2)] public string Message { get; set; } = string.Empty;
}

[GenerateShape]
public sealed partial class BoolReply : ReplyBase
{
    [Key(1)] public bool Result { get; set; }
}
