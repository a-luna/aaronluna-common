namespace AaronLuna.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using AaronLuna.Common.Result;

    public static class StringExtensionsWithTuples
    {
        public static Result<List<string>> GetAllIPv4AddressesInString(this string input)
        {
            if (input == null)
            {
                return Result.Fail<List<string>>("RegEx string cannot be null");
            }

            const string Pattern =
                @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";

            bool isValid;
            var ips = new List<string>();

            try
            {
                var match = Regex.Match(input, Pattern);
                isValid = match.Success;

                if (isValid)
                {
                    while (match.Success)
                    {
                        ips.Add(match.Value);
                        match = match.NextMatch();
                    }
                }
            }
            catch (RegexMatchTimeoutException regexMatchTimeoutException)
            {
                var exceptionType = regexMatchTimeoutException.GetType();
                var exceptionMessage = regexMatchTimeoutException.Message;

                return Result.Fail<List<string>>($"{exceptionMessage} ({exceptionType})");
            }
            catch (ArgumentException argumentException)
            {
                var exceptionType = argumentException.GetType();
                var exceptionMessage = argumentException.Message;

                return Result.Fail<List<string>>($"{exceptionMessage} ({exceptionType})");
            }

            return Result.Ok(ips);
        }
    }
}
