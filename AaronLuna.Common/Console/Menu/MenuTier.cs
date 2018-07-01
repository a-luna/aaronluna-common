namespace AaronLuna.Common.Console.Menu
{
    using System.Collections.Generic;

    public class MenuTier
    {
        public string TierLabel { get; set; }
        public List<IMenuItem> MenuItems {get; set; }
    }
}
