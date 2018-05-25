namespace AaronLuna.Common.Console.Menu
{
    using System.Threading.Tasks;

    using Result;

    public class ReturnToParentMenuItem : IMenuItem
    {
        public ReturnToParentMenuItem(string itemText)
        {
            ReturnToParent = true;
            ItemText = itemText;
        }

        public string ItemText { get; set; }
        public bool ReturnToParent { get; set; }

        public Task<Result> ExecuteAsync()
        {
            return Task.Factory.StartNew(Execute);
        }

        Result Execute()
        {
            return Result.Ok();
        }
    }
}
