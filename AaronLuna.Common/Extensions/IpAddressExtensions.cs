﻿namespace AaronLuna.Common.Extensions
{
    using System.Net;

    using Network;
    using Result;

    public static class IpAddressExtensions
    {
        public static Result<bool> IsInRange(this IPAddress ip, string cidrIp)
        {
            var checkRangeResult = NetworkUtilities.IpAddressIsInRange(ip, cidrIp);

            return checkRangeResult.Success
                ? Result.Ok(checkRangeResult.Value)
                : Result.Fail<bool>(checkRangeResult.Error);
        }

        public static bool IsPrivateAddress(this IPAddress ip)
        {
            return NetworkUtilities.IpAddressIsInPrivateAddressSpace(ip);
        }

        public static bool IsPublicAddress(this IPAddress ip)
        {
            return !NetworkUtilities.IpAddressIsInPrivateAddressSpace(ip);
        }

        public static string ConvertToBinaryString(this IPAddress ip, bool separateBytes)
        {
            return NetworkUtilities.ConvertIpAddressToBinary(ip, separateBytes);
        }
    }
}
