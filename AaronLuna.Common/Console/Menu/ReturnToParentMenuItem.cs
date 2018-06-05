namespace AaronLuna.Common.Console.Menu
{
    using System;
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
            return Task.Run((Func<Result>) Execute);
        }

        Result Execute()
        {
            return Result.Ok();
        }
    }
}
