namespace AaronLuna.Common.Result
{
    public class SimpleError : Error
    {
        public SimpleError(string message)
        {
            Message = message;
        }

        public override ErrorType Type => ErrorType.Simple;

        public string Message { get; }        
    }
}
