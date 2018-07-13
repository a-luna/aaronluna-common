using AaronLuna.Common.Extensions;

namespace AaronLuna.Common.Console.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Result;

    public static class ConsoleMenu
    {
        public static void DisplayMenu(string menuText, List<IMenuItem> menuItems)
        {
            Console.WriteLine($"{menuText}{Environment.NewLine}");
            foreach (var i in Enumerable.Range(0, menuItems.Count))
            {
                Console.WriteLine($"{i + 1}. {menuItems[i].ItemText}");
            }
        }

        public static void DisplayTieredMenu(TieredMenu tieredMenu)
        {
            var itemCount = 1;
            foreach (var i in Enumerable.Range(0, tieredMenu.TierCount))
            {
                var menuItemCountThisTier = tieredMenu.GetMenuItemCountForTier(i);
                if (menuItemCountThisTier == 0) continue;

                var thisTierLabel = tieredMenu.GetTierLabel(i);
                if (!string.IsNullOrEmpty(thisTierLabel))
                {
                    Console.WriteLine(thisTierLabel + Environment.NewLine);
                }
                
                foreach (var j in Enumerable.Range(0, menuItemCountThisTier))
                {
                    var newLineOnLastMenuItem = j.IsLastIteration(menuItemCountThisTier)
                        ? Environment.NewLine
                        : string.Empty;

                    var menuItemText =
                        $"{itemCount}. {tieredMenu.GetMenuItem(itemCount - 1).ItemText}" +
                        newLineOnLastMenuItem;

                    Console.WriteLine(menuItemText);
                    itemCount++;
                }
            }
        }

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
