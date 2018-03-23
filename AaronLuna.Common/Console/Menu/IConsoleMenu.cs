namespace AaronLuna.Common.Console.Menu
{
    using System.Collections.Generic;

    public interface IConsoleMenu : ICommand
    {
        string MenuText { get; set; }
        List<ICommand> Options { get; set; }
    }
}
