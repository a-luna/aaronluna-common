namespace AaronLuna.Common.Console.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Result;

    public static class Menu
    {
        public static ICommand GetUserSelection(string menuText, List<ICommand> menuOptions)
        {
            var userSelection = 0;
            while (userSelection == 0)
            {
                DisplayMenu(menuText, menuOptions);
                var input = Console.ReadLine();

                var validationResult = ValidateUserInput(input, menuOptions.Count);
                if (validationResult.Failure)
                {
                    Console.WriteLine(validationResult.Error);
                    continue;
                }

                userSelection = validationResult.Value;
            }

            return menuOptions[userSelection - 1];
        }

        public static void DisplayMenu(string menuText, List<ICommand> menuOptions)
        {
            Console.Clear();
            Console.WriteLine(menuText);
            Console.WriteLine();
            foreach (var i in Enumerable.Range(0, menuOptions.Count))
            {
                Console.WriteLine($"{i + 1}. {menuOptions[i].ItemText}");
            }
        }

        public static Result<int> ValidateUserInput(string input, int rangeMax)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Result.Fail<int>("Error! Input was null or empty string.");
            }

            if (!int.TryParse(input, out var parsedNum))
            {
                return Result.Fail<int>($"Unable to parse int value from input string: {input}");
            }

            if (0 >= parsedNum || parsedNum > rangeMax)
            {
                return Result.Fail<int>($"{parsedNum} is not within allowed range {1}-{rangeMax}");
            }

            return Result.Ok(parsedNum);
        }
    }
}
