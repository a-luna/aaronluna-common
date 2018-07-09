namespace AaronLuna.Common.Console.Menu
{
    public interface ITieredMenu : IMenuItem
    {
        string MenuText { get; set; }
        TieredMenu Menu { get; set; }
    }
}
