using Nerdbank.MessagePack;
using PolyType;

namespace XGrpcShared
{
    [GenerateShape]
    public sealed partial class ErrorResult : ResultBase
    {
        [Key(1)]
        public ExcpCode Code { get; set; }

        [Key(2)]
        public string Message { get; set; } = string.Empty;
    }
}
