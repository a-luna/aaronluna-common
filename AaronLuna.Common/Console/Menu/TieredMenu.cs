namespace AaronLuna.Common.Console.Menu
{
    using System.Collections.Generic;

    public class TieredMenu
    {
        readonly List<IMenuItem> _menuItems;

        public TieredMenu()
        {
            _menuItems = new List<IMenuItem>();
            Tiers = new List<MenuTier>();
        }

        public List<MenuTier> Tiers { get; }
        public int Count => _menuItems.Count;

        public void Add(MenuTier menuTier)
        {
            Tiers.Add(menuTier);
            _menuItems.AddRange(menuTier.MenuItems);
        }

        public void Clear()
        {
            Tiers.Clear();
            _menuItems.Clear();
        }

        public IMenuItem GetMenuItem(int index)
        {
            if (_menuItems.Count == 0) return null;
            if (0 > index || index >= _menuItems.Count) return null;

            return _menuItems[index];
        }
    }
}
