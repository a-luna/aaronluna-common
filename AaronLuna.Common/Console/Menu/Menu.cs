namespace AaronLuna.Common.Console.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Result;

    public static class Menu
    {
        public static IMenuItem GetUserSelection(string menuText, List<IMenuItem> menuItems)
        {
            var userSelection = 0;
            while (userSelection == 0)
            {
                DisplayMenu(menuText, menuItems);
                var input = Console.ReadLine();

                var validationResult = ValidateUserInput(input, menuItems.Count);
                if (validationResult.Failure)
                {
                    Console.WriteLine(validationResult.Error);
                    continue;
                }

                userSelection = validationResult.Value;
            }

            return menuItems[userSelection - 1];
        }

        public static void DisplayTieredMenu(TieredMenu tieredMenu)
        {
            var itemCount = 1;

            foreach (var i in Enumerable.Range(0, tieredMenu.Tiers.Count))
            {
                if (tieredMenu.Tiers[i].MenuItems.Count == 0) continue;

                if (!string.IsNullOrEmpty(tieredMenu.Tiers[i].TierLabel))
                {
                    Console.WriteLine($"{tieredMenu.Tiers[i].TierLabel}{Environment.NewLine}");
                }
                
                foreach (var j in Enumerable.Range(0, tieredMenu.Tiers[i].MenuItems.Count))
                {
                    Console.WriteLine($"{itemCount}. {tieredMenu.Tiers[i].MenuItems[j].ItemText}");
                    itemCount++;
                }

                Console.WriteLine(Environment.NewLine);
            }
        }

        public static void DisplayMenu(string menuText, List<IMenuItem> menuItems)
        {
            Console.WriteLine($"{menuText}{Environment.NewLine}");
            foreach (var i in Enumerable.Range(0, menuItems.Count))
            {
                Console.WriteLine($"{i + 1}. {menuItems[i].ItemText}");
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
                return Result.Fail<int>($"{parsedNum} is not within allowed range 1-{rangeMax}");
            }

            return Result.Ok(parsedNum);
        }
    }
}
