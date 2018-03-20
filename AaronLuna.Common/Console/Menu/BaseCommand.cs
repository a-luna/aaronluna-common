namespace AaronLuna.Common.Console.Menu
{
    using System.Net;
    using System.Threading.Tasks;

    using Result;

    public abstract class BaseCommand<T> : ICommand<T>
    {
        readonly string _itemText;
        readonly bool _returnToParent;

        protected BaseCommand(string itemText, bool returnToParent)
        {
            _itemText = itemText;
            _returnToParent = returnToParent;
        }

        public abstract Task<Result<T>> ExecuteAsync();
        public string GetItemText() { return _itemText; }
        public bool ReturnToParent() { return _returnToParent; }
    }

    public abstract class BoolCommand : IBoolCommand
    {
        readonly string _itemText;
        readonly bool _returnToParent;

        protected BoolCommand(string itemText, bool returnToParent)
        {
            _itemText = itemText;
            _returnToParent = returnToParent;
        }

        public abstract Task<Result<bool>> ExecuteAsync();
        public string GetItemText() { return _itemText; }
        public bool ReturnToParent() { return _returnToParent; }
    }

    public abstract class IntCommand : IIntCommand
    {
        readonly string _itemText;
        readonly bool _returnToParent;

        protected IntCommand(string itemText)
        {
            _itemText = itemText;
            _returnToParent = false;
        }
        
        public abstract Task<Result<int>> ExecuteAsync();
        public string GetItemText() { return _itemText; }
        public bool ReturnToParent() { return _returnToParent; }
    }

    public abstract class StringCommand : IStringCommand
    {
        readonly string _itemText;
        readonly bool _returnToParent;

        protected StringCommand(string itemText)
        {
            _itemText = itemText;
            _returnToParent = false;
        }

        public abstract Task<Result<string>> ExecuteAsync();
        public string GetItemText() { return _itemText; }
        public bool ReturnToParent() { return _returnToParent; }
    }

    public abstract class IpAddressCommand : IIpAddressCommand
    {
        readonly string _itemText;
        readonly bool _returnToParent;

        protected IpAddressCommand(string itemText)
        {
            _itemText = itemText;
            _returnToParent = false;
        }

        public abstract Task<Result<IPAddress>> ExecuteAsync();
        public string GetItemText() { return _itemText; }
        public bool ReturnToParent() { return _returnToParent; }
    }
}
