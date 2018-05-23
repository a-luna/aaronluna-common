namespace AaronLuna.Common.Console.Menu
{
    using System.Collections.Generic;

    public interface IMenu : IMenuItem
    {
        string MenuText { get; set; }
        List<IMenuItem> MenuItems { get; set; }
    }
}
