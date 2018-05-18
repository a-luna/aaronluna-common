namespace AaronLuna.Common.Console.Menu
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Result;

    public abstract class MenuSingleChoice : ICommand
    {
        protected MenuSingleChoice() { }

        protected MenuSingleChoice(string itemText, List<ICommand> menuOptions)
        {
            ReturnToParent = true;
            ItemText = itemText;
            MenuOptions = menuOptions;
        }

        public string ItemText { get; set; }
        public bool ReturnToParent { get; set; }
        public string MenuText { get; set; }
        public List<ICommand> MenuOptions { get; set; }

        public async Task<Result> ExecuteAsync()
        {
            var selectedOption = Menu.GetUserSelection(MenuText, MenuOptions);
            return await selectedOption.ExecuteAsync().ConfigureAwait(false);
        }
    }
}
