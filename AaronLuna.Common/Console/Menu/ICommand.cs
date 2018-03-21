namespace AaronLuna.Common.Console.Menu
{
    using System.Threading.Tasks;

    public interface ICommand<T>
    {
        string ItemText { get; set; }
        bool ReturnToParent { get; set; }

        Task<CommandResult<T>> ExecuteAsync();
    }
}
