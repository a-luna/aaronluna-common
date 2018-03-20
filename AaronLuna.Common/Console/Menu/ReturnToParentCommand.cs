namespace AaronLuna.Common.Console.Menu
{
    using System.Threading.Tasks;

    using Result;

    public class ReturnToParentCommand : BoolCommand
    {
        public ReturnToParentCommand(string itemText) : base(itemText, true) { }

        public override async Task<Result<bool>> ExecuteAsync()
        {
            await Task.Delay(1);
            return Result.Ok(true);
        }
    }
}
