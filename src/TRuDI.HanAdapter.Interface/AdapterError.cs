namespace TRuDI.HanAdapter.Interface
{
    public class AdapterError
    {
        public AdapterError(ErrorType type)
        {
            this.Type = type;
        }

        public AdapterError(ErrorType type, string message)
        {
            this.Type = type;
            this.Message = message;
        }

        public ErrorType Type { get; }

        public string Message { get; }
    }
}