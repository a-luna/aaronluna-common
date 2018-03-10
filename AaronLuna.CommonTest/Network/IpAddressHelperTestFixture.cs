namespace AaronLuna.CommonTest.Network
{
    using Common.Network;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using System.Collections.Generic;
    using System.Net;

    [TestClass]
    public class IpAddressHelperTestFixture
    {
        [TestMethod]
        public void ParseValidString()
        {
            const string input = "192.168.2.9";
            var result = Network.ParseAllIPv4Addresses(input);
            var parsedIps = new List<IPAddress>();
            var parsedIp = string.Empty;

            if (result.Success)
            {
                parsedIps = result.Value;
            }

            if (parsedIps.Count > 0)
            {
                parsedIp = parsedIps[0].ToString();
            }

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, parsedIps.Count);
            Assert.AreEqual(input, parsedIp);
        }
    }
}
