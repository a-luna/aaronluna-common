namespace AaronLuna.Common.Console.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Result;

    public class SelectionMenu<T> : BaseCommand<T>
    {
        public SelectionMenu(string itemText, List<ICommand<T>> options)
            : base(itemText, false)
        {
            Options = options;
        }

        public string MenuText { get; set; }
        public List<ICommand<T>> Options { get; set; }
        public int OptionCount => Options.Count;

        public override async Task<Result<T>> ExecuteAsync()
        {
            var exit = false;
            Result<T> result = null;

            while (!exit)
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
                result = await selectedOption.ExecuteAsync();
                exit = selectedOption.ReturnToParent();

                if (result.Success) continue;
                Console.WriteLine(result.Error);
                exit = true;
            }

            return result;
        }

        void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine(MenuText);
            Console.WriteLine();
            foreach (var i in Enumerable.Range(0, OptionCount))
            {
                Console.WriteLine($"{i + 1}.{Options[i].GetItemText()}");
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
