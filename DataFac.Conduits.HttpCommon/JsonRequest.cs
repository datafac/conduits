namespace DataFac.Conduits.HttpCommon;

public class JsonRequest
{
    public long? DeadlineUtc { get; set; }
    public byte[]? Payload { get; set; }
    public int Route { get; set; }
}
