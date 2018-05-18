namespace AaronLuna.Common.Console.Menu
{
    using System.Collections.Generic;

    public interface IMenu : ICommand
    {
        string MenuText { get; set; }
        List<ICommand> MenuOptions { get; set; }
    }
}
