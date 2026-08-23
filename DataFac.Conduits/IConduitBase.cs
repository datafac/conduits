using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataFac.Conduits
{
    public interface IConduitBase
    {
        ValueTask<ReadOnlyMemory<byte>> SimpleUnaryCall(ReadOnlyMemory<byte> request, CallContext context);
        IAsyncEnumerable<ReadOnlyMemory<byte>> ServerStream(ReadOnlyMemory<byte> request, CallContext context);
        ValueTask<ReadOnlyMemory<byte>> ClientStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, CallContext context);
        IAsyncEnumerable<ReadOnlyMemory<byte>> DuplexStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, CallContext context);
    }
    public interface IConduitServer : IConduitBase
    {
        string ServerName { get; }
        string ServerVersion { get; }
    }
    public interface IConduitClient : IConduitBase
    {

    }
}