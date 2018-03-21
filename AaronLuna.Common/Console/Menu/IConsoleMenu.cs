namespace AaronLuna.Common.Console.Menu
{
    using System.Collections.Generic;

    public interface IConsoleMenu<T> : ICommand<T>
    {
        string MenuText { get; set; }
        List<ICommand<T>> Options { get; set; }
        int OptionCount { get; }

        void DisplayMenu();

    }
}
