namespace AaronLuna.Common.Console.Menu
{
    using System.Collections.Generic;
    using System.Linq;

    using Result;

    public static class MenuFunctions
    {
        public static void DisplayMenu(string menuText, List<ICommand> menuOptions)
        {
            System.Console.Clear();
            System.Console.WriteLine(menuText);
            System.Console.WriteLine();
            foreach (var i in Enumerable.Range(0, menuOptions.Count))
            {
                System.Console.WriteLine($"{i + 1}. {menuOptions[i].ItemText}");
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

            if (parsedNum <= 0 || parsedNum > rangeMax)
            {
                return Result.Fail<int>($"{parsedNum} is not within allowed range {1}-{rangeMax}");
            }

            return Result.Ok(parsedNum);
        }
    }
}
