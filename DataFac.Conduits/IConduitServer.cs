using System;

namespace DataFac.Conduits;

public interface IConduitServer : IConduitBase, IAsyncDisposable
{
    TimeProvider TimeProvider { get; }
    string ServerName { get; }
    string ServerVersion { get; }
}
