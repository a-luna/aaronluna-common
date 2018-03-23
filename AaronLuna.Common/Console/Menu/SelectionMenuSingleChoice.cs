namespace AaronLuna.Common.Console.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Result;

    public abstract class SelectionMenuSingleChoice : ICommand
    {
        protected SelectionMenuSingleChoice() { }

        protected SelectionMenuSingleChoice(string itemText, List<ICommand> options)
        {
            ReturnToParent = true;
            ItemText = itemText;
            Options = options;
        }

        public string ItemText { get; set; }
        public bool ReturnToParent { get; set; }
        public string MenuText { get; set; }
        public List<ICommand> Options { get; set; }

        public async Task<Result> ExecuteAsync()
        {
            var userSelection = 0;
            while (userSelection == 0)
            {
                MenuFunctions.DisplayMenu(MenuText, Options);
                var input = Console.ReadLine();

                var validationResult = MenuFunctions.ValidateUserInput(input, Options.Count);
                if (validationResult.Failure)
                {
                    Console.WriteLine(validationResult.Error);
                    continue;
                }

                userSelection = validationResult.Value;
            }

            var selectedOption = Options[userSelection - 1];

            await Task.Delay(1);
            return await selectedOption.ExecuteAsync();
        }
    }
}
