namespace AaronLuna.Common.Console.Menu
{
    using System.Net;
    using System.Threading.Tasks;

    using Result;

    public interface ICommand<T>
    {
        Task<Result<T>> ExecuteAsync();
        string GetItemText();
        bool ReturnToParent();
    }

    public interface IBoolCommand : ICommand<bool> { }
    public interface IIntCommand : ICommand<int> { }
    public interface IStringCommand : ICommand<string> { }
    public interface IIpAddressCommand : ICommand<IPAddress> { }
}
