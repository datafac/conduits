using System.Threading.Tasks;

namespace DataFac.Conduits;

public interface IConduitInfo
{
    ValueTask<ConduitInfo> GetConduitInfoAsync();
}
