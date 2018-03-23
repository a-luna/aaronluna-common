﻿namespace AaronLuna.Common.Console.Menu
{
    using Result;

    public class CommandResult
    {
        public Result Result { get; set; }
        public bool ReturnToParent { get; set; }
    }

    public class CommandResult<T>
    {
        public Result<T> Result { get; set; }
        public bool ReturnToParent { get; set; }
    }
}
