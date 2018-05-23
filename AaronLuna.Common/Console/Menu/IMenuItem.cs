namespace AaronLuna.Common.Console.Menu
{
    using System.Threading.Tasks;
    using Result;

    public interface IMenuItem
    {
        string ItemText { get; set; }
        bool ReturnToParent { get; set; }

        Task<Result> ExecuteAsync();
    }
}
