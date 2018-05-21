namespace AaronLuna.Common.Console.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Result;

    public abstract class MenuLoop : ICommand
    {
        protected MenuLoop()
        {
            ReturnToParent = false;
            ItemText = string.Empty;
            MenuOptions = new List<ICommand>();
        }

        protected MenuLoop(string itemText, List<ICommand> menuOptions)
        {
            ReturnToParent = false;
            ItemText = itemText;
            MenuOptions = menuOptions;
        }

        public string ItemText { get; set; }
        public bool ReturnToParent { get; set; }
        public string MenuText { get; set; }
        public List<ICommand> MenuOptions { get; set; }

        public async Task<Result> ExecuteAsync()
        {
            var exit = false;
            Result result = null;

            while (!exit)
            {
                var selectedOption = Menu.GetUserSelection(MenuText, MenuOptions);
                exit = selectedOption.ReturnToParent;
                result = await selectedOption.ExecuteAsync().ConfigureAwait(false);

                if (result.Success) continue;
                Console.WriteLine(result.Error);
                exit = true;
            }

            return result;
        }
    }
}
