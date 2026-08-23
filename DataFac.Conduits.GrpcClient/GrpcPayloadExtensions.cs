using Google.Protobuf;
using DataFac.Conduits.GrpcCommon;
using System;

namespace DataFac.Conduits.GrpcClient
{
    internal static class GrpcPayloadExtensions
    {
        public static GrpcPayload ToGrpcPayload(this ReadOnlyMemory<byte> input)
        {
            return new GrpcPayload() { Data = UnsafeByteOperations.UnsafeWrap(input) };
        }
        public static ReadOnlyMemory<byte> ToPayload(this GrpcPayload input)
        {
            return input.Data.Memory;
        }
    }
}