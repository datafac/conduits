using ProtoBuf;
using System;

namespace XGrpcShared
{
    [ProtoContract]
    public sealed class RequestBlob
    {
        [ProtoMember(1)]
        public byte[] Blob { get; set; } = Array.Empty<byte>();
    }
}
