namespace AaronLuna.Common.Console.Menu
{
    using System.Threading.Tasks;

    using Result;

    public class ReturnToParentCommand<T> : ICommand<T>
    {
        public ReturnToParentCommand() { }

        public ReturnToParentCommand(string itemText)
        {
            ReturnToParent = true;
            ItemText = itemText;
        }

        public string ItemText { get; set; }
        public bool ReturnToParent { get; set; }

        public async Task<CommandResult<T>> ExecuteAsync()
        {
            await Task.Delay(1);

            return new CommandResult<T>
            {
                ReturnToParent = ReturnToParent,
                Result = (Result<T>)Result.Ok()
            };
        }
    }
}
