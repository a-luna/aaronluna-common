namespace AaronLuna.Common.Console.Menu
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Result;
    public class CommandSequence<T> : BaseCommand<T>
    {
        public CommandSequence(string itemText, List<ICommand<T>> commands) : base(itemText, false)
        {
            Commands = commands;
        }
        
        public List<ICommand<T>> Commands { get; set; }

        public override async Task<Result<T>> ExecuteAsync()
        {
            Result<T> result = null;
            foreach (var command in Commands)
            {
                result = await command.ExecuteAsync();
                if (result.Success) continue;

                Console.WriteLine(result.Error);
                break;
            }

            return result;
        }
    }
}
