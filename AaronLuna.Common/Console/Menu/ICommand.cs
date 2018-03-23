using System.Collections.Generic;

namespace AaronLuna.Common.Console.Menu
{
    using System.Threading.Tasks;
    using Result;

    public interface ICommand
    {
        string ItemText { get; set; }
        bool ReturnToParent { get; set; }

        Task<Result> ExecuteAsync();
    }
}
