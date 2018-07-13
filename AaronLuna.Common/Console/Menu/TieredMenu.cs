namespace AaronLuna.Common.Console.Menu
{
    using System.Collections.Generic;

    public class TieredMenu
    {
        readonly List<MenuTier> _tiers;
        readonly List<IMenuItem> _menuItems;

        public TieredMenu()
        {
            _menuItems = new List<IMenuItem>();
            _tiers = new List<MenuTier>();
        }
        
        public int ItemCount => _menuItems.Count;
        public int TierCount => _tiers.Count;

        public void AddTier(MenuTier menuTier)
        {
            _tiers.Add(menuTier);
            _menuItems.AddRange(menuTier.MenuItems);
        }

        public string GetTierLabel(int index)
        {
            return _tiers[index].TierLabel;
        }
        
        public IMenuItem GetMenuItem(int index)
        {
            if (_menuItems.Count == 0) return null;
            if (0 > index || index >= _menuItems.Count) return null;

            return _menuItems[index];
        }

        public int GetMenuItemCountForTier(int index)
        {
            return _tiers[index].MenuItems.Count;
        }

        public void Clear()
        {
            _tiers.Clear();
            _menuItems.Clear();
        }
    }
}
