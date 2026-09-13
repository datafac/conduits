namespace DataFac.Conduits.HttpCommon;

public class JsonResponse
{
    public int ControlCode { get; set; }
    public byte[]? Payload { get; set; }
}
