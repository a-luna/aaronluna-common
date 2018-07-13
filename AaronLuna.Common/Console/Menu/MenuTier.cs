namespace AaronLuna.Common.Console.Menu
{
    using System.Collections.Generic;

    public class MenuTier
    {
        public MenuTier(string tierLabel)
        {
            TierLabel = tierLabel;
            MenuItems = new List<IMenuItem>();
        }

        public string TierLabel { get; set; }
        public List<IMenuItem> MenuItems {get; set; }
    }
}
