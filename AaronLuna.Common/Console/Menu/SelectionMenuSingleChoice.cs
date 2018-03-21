namespace AaronLuna.Common.Console.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Result;

    public abstract class SelectionMenuSingleChoice<T> : IConsoleMenu<T>
    {
        protected SelectionMenuSingleChoice() { }

        protected SelectionMenuSingleChoice(string itemText, List<ICommand<T>> options)
        {
            ReturnToParent = false;
            ItemText = itemText;
            Options = options;
        }

        public string ItemText { get; set; }
        public bool ReturnToParent { get; set; }
        public string MenuText { get; set; }
        public List<ICommand<T>> Options { get; set; }
        public int OptionCount => Options.Count;

        public async Task<CommandResult<T>> ExecuteAsync()
        {
            var userSelection = 0;
            while (userSelection == 0)
            {
                DisplayMenu();
                var input = Console.ReadLine();

                var validationResult = ValidateUserInput(input, OptionCount);
                if (validationResult.Failure)
                {
                    Console.WriteLine(validationResult.Error);
                    continue;
                }

                userSelection = validationResult.Value;
            }

            var selectedOption = Options[userSelection - 1];
            var result = await selectedOption.ExecuteAsync();
            if (result == null) throw new ArgumentNullException(nameof(result));

            return new CommandResult<T>
            {
                ReturnToParent = ReturnToParent,
                Result = result.Result
            };
        }

        public void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine(MenuText);
            Console.WriteLine();
            foreach (var i in Enumerable.Range(0, OptionCount))
            {
                Console.WriteLine($"{i + 1}.{Options[i].ItemText}");
            }
        }

        static Result<int> ValidateUserInput(string input, int rangeMax)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Result.Fail<int>("Error! Input was null or empty string.");
            }

            if (!int.TryParse(input, out var parsedNum))
            {
                return Result.Fail<int>($"Unable to parse int value from input string: {input}");
            }

            if (parsedNum <= 0 || parsedNum > rangeMax)
            {
                return Result.Fail<int>($"{parsedNum} is not within allowed range {1}-{rangeMax}");
            }

            return Result.Ok(parsedNum);
        }
    }
}
