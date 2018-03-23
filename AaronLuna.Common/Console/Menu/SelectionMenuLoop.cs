namespace AaronLuna.Common.Console.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Result;

    public abstract class SelectionMenuLoop : ICommand
    {
        protected SelectionMenuLoop()
        {
            ReturnToParent = false;
            ItemText = string.Empty;
            Options = new List<ICommand>();
        }

        protected SelectionMenuLoop(string itemText, List<ICommand> options)
        {
            ReturnToParent = false;
            ItemText = itemText;
            Options = options;
        }

        public string ItemText { get; set; }
        public bool ReturnToParent { get; set; }
        public string MenuText { get; set; }
        public List<ICommand> Options { get; set; }
        public int OptionCount => Options.Count;

        public async Task<Result> ExecuteAsync()
        {
            var exit = false;
            Result result = null;

            while (!exit)
            {
                var userSelection = 0;
                while (userSelection == 0)
                {
                    MenuFunctions.DisplayMenu(MenuText, Options);
                    var input = Console.ReadLine();

                    var validationResult = MenuFunctions.ValidateUserInput(input, OptionCount);
                    if (validationResult.Failure)
                    {
                        Console.WriteLine(validationResult.Error);
                        continue;
                    }

                    userSelection = validationResult.Value;
                }

                var selectedOption = Options[userSelection - 1];
                result = await selectedOption.ExecuteAsync();
                exit = selectedOption.ReturnToParent;

                if (result.Success) continue;
                Console.WriteLine(result.Error);
                exit = true;
            }

            return result;
        }
    }
}
