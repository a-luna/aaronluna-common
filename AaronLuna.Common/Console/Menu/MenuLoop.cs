namespace AaronLuna.Common.Console.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Result;

    public abstract class MenuLoop : IMenu
    {
        protected MenuLoop()
        {
            ReturnToParent = false;
            ItemText = string.Empty;
            MenuItems = new List<IMenuItem>();
        }

        public string ItemText { get; set; }
        public bool ReturnToParent { get; set; }
        public string MenuText { get; set; }
        public List<IMenuItem> MenuItems { get; set; }

        public async Task<Result> ExecuteAsync()
        {
            var exit = false;
            Result result = null;

            while (!exit)
            {
                var menuItem = Menu.GetUserSelection(MenuText, MenuItems);
                result = await menuItem.ExecuteAsync().ConfigureAwait(false);

                exit = menuItem.ReturnToParent;
                if (result.Success) continue;

                Console.WriteLine(result.Error);
                exit = true;
            }

            return result;
        }
    }
}
