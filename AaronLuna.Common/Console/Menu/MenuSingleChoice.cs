namespace AaronLuna.Common.Console.Menu
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Result;

    public abstract class MenuSingleChoice : IMenu
    {
        protected MenuSingleChoice() { }

        protected MenuSingleChoice(string itemText, List<IMenuItem> menuOptions)
        {
            ReturnToParent = true;
            ItemText = itemText;
            MenuItems = menuOptions;
        }

        public string ItemText { get; set; }
        public bool ReturnToParent { get; set; }
        public string MenuText { get; set; }
        public List<IMenuItem> MenuItems { get; set; }

        public Task<Result> ExecuteAsync()
        {
            var selectedOption = Menu.GetUserSelection(MenuText, MenuItems);
            return selectedOption.ExecuteAsync();
        }
    }
}
