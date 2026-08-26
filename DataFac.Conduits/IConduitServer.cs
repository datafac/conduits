using System;

namespace DataFac.Conduits;

public interface IConduitServer : IConduitBase, IAsyncDisposable
{
    string ServerName { get; }
    string ServerVersion { get; }
}
