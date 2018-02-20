namespace AaronLuna.CommonTest.Network
{
    using System.Linq;

    using AaronLuna.Common.Extensions;
    using AaronLuna.Common.Network;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IpAddressHelperTestFixture
    {
        [TestMethod]
        public void ParseValidString()
        {
            var input = "192.168.2.9";
            var result = IpAddressHelper.ParseAllIPv4Addresses(input);
            var parsedIps = result.Value;

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, parsedIps.Count);
            Assert.AreEqual(input, parsedIps.FirstOrDefault().ToString());
        }
    }
}
