using System;

namespace DataFac.Conduits;

public interface IConduitClient : IConduitBase, IAsyncDisposable
{
    TimeProvider TimeProvider { get; }
}