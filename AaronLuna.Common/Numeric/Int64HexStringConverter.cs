namespace AaronLuna.Common.Numeric
{
    using System;
    using System.Globalization;

    using Extensions;
    using Result;

    public struct Int64HexStringConverter
    {
        readonly string _hexString;
        readonly bool _isNegativeNumber;

        public Int64HexStringConverter(string hexString, bool isSignedType)
        {
            _hexString = hexString;
            _isNegativeNumber = isSignedType && hexString.HexStringRepresentsNegativeNumber();
        }

        public Result<long> ConvertToSignedInt64()
        {
            if (string.IsNullOrEmpty(_hexString))
            {
                return Result.Fail<long>("Input string was null or empty. (ArgumentNullException)");
            }

            // ReSharper disable once ExceptionNotDocumentedOptional
            // "style" parameter in TryParse call is hardcoded to a valid NumberStyles 
            // value, thus there is no reason to check for the conditions that would 
            // throw an ArgumentException.
            if (long.TryParse(_hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var signedInt64))
            {
                try
                {
                    // Throw an exception if the hexstring represents a positive value but
                    // the parsed value is interpereted as negative
                    if (!_isNegativeNumber && signedInt64.ToString("X").HexStringRepresentsNegativeNumber())
                    {
                        throw new OverflowException($"{_hexString} cannot be converted to Int64");
                    }

                    return Result.Ok(signedInt64);
                }
                catch (OverflowException overflowException)
                {
                    var exceptionType = overflowException.GetType();
                    var exceptionMessage = overflowException.Message;

                    return Result.Fail<long>($"{exceptionMessage} ({exceptionType})");
                }
            }

            // This check makes sure that the Convert.ToInt64 call in the try block
            // below will not throw an ArgumentException by ensuring that the input
            // string is not prefixed with a negative sign.
            //
            // The input string has been checked against null at the beginning of this
            // method, so the StartsWith() method call below will not throw an 
            // ArgumentNullException.
            //
            // "StringComparison" paremeter in the StartsWith() method call is
            // hardcoded to a valid StringComparison value, so there is no need to 
            // check for the condition that would throw an ArgumentException.
            if (_hexString.StartsWith("-", StringComparison.InvariantCulture))
            {
                return Result.Fail<long>("Input string cannot contain a minus sign if the base is not 10. (ArgumentException)");
            }

            try
            {
                // Convert the string to an Int64
                signedInt64 = Convert.ToInt64(_hexString, 16);

                // Throw an exception if the hexstring represents a positive value but
                // the parsed value is interpereted as negative
                if (!_isNegativeNumber && signedInt64.ToString("X").HexStringRepresentsNegativeNumber())
                {
                    throw new OverflowException($"{_hexString} cannot be converted to Int64");
                }

                return Result.Ok(signedInt64);
            }
            catch (FormatException formatException)
            {
                var exceptionType = formatException.GetType();
                var exceptionMessage = formatException.Message;

                return Result.Fail<long>($"{exceptionMessage} ({exceptionType})");
            }
            catch (OverflowException overflowException)
            {
                var exceptionType = overflowException.GetType();
                var exceptionMessage = overflowException.Message;

                return Result.Fail<long>($"{exceptionMessage} ({exceptionType})");
            }
        }

        public Result<ulong> ConvertToUnsignedInt64()
        {
            if (string.IsNullOrEmpty(_hexString))
            {
                return Result.Fail<ulong>("Input string was null or empty. (ArgumentNullException)");
            }

            // ReSharper disable once ExceptionNotDocumentedOptional
            // "style" parameter in TryParse call is hardcoded to a valid NumberStyles 
            // value, thus there is no reason to check for the conditions that would 
            // throw an ArgumentException.
            if (ulong.TryParse(_hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var unsignedInt64))
            {
                try
                {
                    // Throw an exception if the hexstring represents a negative value but
                    // the parsed value is interpereted as positive
                    if (_isNegativeNumber)
                    {
                        throw new OverflowException(
                            $"{_hexString} cannot be converted to UInt64. Converted value of {unsignedInt64:N0} is positive and hex string repsresents a negative number.");
                    }

                    return Result.Ok(unsignedInt64);
                }
                catch (OverflowException overflowException)
                {
                    var exceptionType = overflowException.GetType();
                    var exceptionMessage = overflowException.Message;

                    return Result.Fail<ulong>($"{exceptionMessage} ({exceptionType})");
                }
            }

            // This check makes sure that the Convert.ToUInt64 call in the try block
            // below will not throw an ArgumentException by ensuring that the input
            // string is not prefixed with a negative sign.
            //
            // The input string has been checked against null at the beginning of this
            // method, so the StartsWith() method call below will not throw an 
            // ArgumentNullException.
            //
            // "StringComparison" paremeter in the StartsWith() method call is
            // hardcoded to a valid StringComparison value, so there is no need to 
            // check for the condition that would throw an ArgumentException.
            if (_hexString.StartsWith("-", StringComparison.InvariantCulture))
            {
                return Result.Fail<ulong>("Input string cannot contain a minus sign if the base is not 10. (ArgumentException)");
            }

            try
            {
                // Convert the string to an Int64
                unsignedInt64 = Convert.ToUInt64(_hexString, 16);

                // Throw an exception if the hexstring represents a negative value but
                // the parsed value is interpereted as positive
                if (_isNegativeNumber)
                {
                    throw new OverflowException($"{_hexString} cannot be converted to UInt64. Converted value of "
                                                + $"{unsignedInt64:N0} is positive and hex string repsresents a negative number.");
                }

                return Result.Ok(unsignedInt64);
            }
            catch (FormatException ex)
            {
                return Result.Fail<ulong>($"{ex.Message} ({ex.GetType()})");
            }
            catch (OverflowException ex)
            {
                return Result.Fail<ulong>($"{ex.Message} ({ex.GetType()})");
            }
        }
    }
}
