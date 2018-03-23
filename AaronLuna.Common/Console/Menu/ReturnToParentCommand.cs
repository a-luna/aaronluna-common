namespace AaronLuna.Common.Console.Menu
{
    using System.Threading.Tasks;

    using Result;

    public class ReturnToParentCommand : ICommand
    {
        public ReturnToParentCommand() { }

        public ReturnToParentCommand(string itemText, bool returnToParent)
        {
            ReturnToParent = returnToParent;
            ItemText = itemText;
        }

        public string ItemText { get; set; }
        public bool ReturnToParent { get; set; }

        public async Task<Result> ExecuteAsync()
        {
            await Task.Delay(1);
            return Result.Ok();
        }
    }
}
