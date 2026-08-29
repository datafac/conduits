using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits;

public interface IConduitServer : IConduitBase, IAsyncDisposable
{
    TimeProvider TimeProvider { get; }
    string ServerName { get; }
    string ServerVersion { get; }
}
