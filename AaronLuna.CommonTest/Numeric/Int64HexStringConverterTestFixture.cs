namespace AaronLuna.CommonTest.Numeric
{
    using AaronLuna.Common.Numeric;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Int64HexStringConverterTestFixture
    {
        [TestMethod]
        public void VerifyNullAndEmptyStrings()
        {
            var hexString1 = new Int64HexStringConverter(string.Empty, true);
            var convertResult1 = hexString1.ConvertToSignedInt64();
            var convertResult1U = hexString1.ConvertToUnsignedInt64();

            Assert.IsTrue(convertResult1.Failure);
            Assert.IsTrue(convertResult1U.Failure);

            var hexString2 = new Int64HexStringConverter(null, true);
            var convertResult2 = hexString2.ConvertToSignedInt64();
            var convertResult2U = hexString2.ConvertToUnsignedInt64();

            Assert.IsTrue(convertResult2.Failure);
            Assert.IsTrue(convertResult2U.Failure);
        }

        [TestMethod]
        public void VerifyUInt64MaxValue()
        {
            var hexString1 = new Int64HexStringConverter($"{ulong.MaxValue:X}", false);
            var convertResult1 = hexString1.ConvertToSignedInt64();
            var convertResult1U = hexString1.ConvertToUnsignedInt64();

            Assert.IsTrue(convertResult1.Failure);
            Assert.IsTrue(convertResult1U.Success);
            Assert.AreEqual(ulong.MaxValue, convertResult1U.Value);

            var hexString2 = new Int64HexStringConverter($"0x{ulong.MaxValue:X}", false);
            var convertResult2 = hexString2.ConvertToSignedInt64();
            var convertResult2U = hexString2.ConvertToUnsignedInt64();

            Assert.IsTrue(convertResult2.Failure);
            Assert.IsTrue(convertResult2U.Success);
            Assert.AreEqual(ulong.MaxValue, convertResult2U.Value);
        }

        [TestMethod]
        public void VerifyInt64MinValue()
        {
            var hexString1 = new Int64HexStringConverter($"{long.MinValue:X}", true);
            var convertResult1 = hexString1.ConvertToSignedInt64();
            var convertResult1U = hexString1.ConvertToUnsignedInt64();

            Assert.IsTrue(convertResult1.Success);
            Assert.AreEqual(long.MinValue, convertResult1.Value);
            Assert.IsTrue(convertResult1U.Failure);

            var hexString2 = new Int64HexStringConverter($"0x{long.MinValue:X}", true);
            var convertResult2 = hexString2.ConvertToSignedInt64();
            var convertResult2U = hexString2.ConvertToUnsignedInt64();

            Assert.IsTrue(convertResult2.Success);
            Assert.AreEqual(long.MinValue, convertResult2.Value);
            Assert.IsTrue(convertResult2U.Failure);
        }

        [TestMethod]
        public void VerifyUInt64GreaterThanInt64MaxValue()
        {
            var sourceNumber = (ulong)long.MaxValue + 256;

            var hexString1 = new Int64HexStringConverter($"{sourceNumber:X}", false);
            var convertResult1 = hexString1.ConvertToSignedInt64();
            var convertResult1U = hexString1.ConvertToUnsignedInt64();

            Assert.IsTrue(convertResult1.Failure);
            Assert.IsTrue(convertResult1U.Success);
            Assert.AreEqual(sourceNumber, convertResult1U.Value);

            var hexString2 = new Int64HexStringConverter($"0x{sourceNumber:X}", false);
            var convertResult2 = hexString2.ConvertToSignedInt64();
            var convertResult2U = hexString2.ConvertToUnsignedInt64();

            Assert.IsTrue(convertResult2.Failure);
            Assert.IsTrue(convertResult2U.Success);
            Assert.AreEqual(sourceNumber, convertResult2U.Value);
        }

        [TestMethod]
        public void VerifyNegativeOne()
        {
            long sourceNumber = -1;

            var hexString1 = new Int64HexStringConverter($"{sourceNumber:X}", true);
            var convertResult1 = hexString1.ConvertToSignedInt64();
            var convertResult1U = hexString1.ConvertToUnsignedInt64();

            Assert.IsTrue(convertResult1.Success);
            Assert.AreEqual(sourceNumber, convertResult1.Value);
            Assert.IsTrue(convertResult1U.Failure);

            var hexString2 = new Int64HexStringConverter($"0x{sourceNumber:X}", true);
            var convertResult2 = hexString2.ConvertToSignedInt64();
            var convertResult2U = hexString2.ConvertToUnsignedInt64();

            Assert.IsTrue(convertResult2.Success);
            Assert.AreEqual(sourceNumber, convertResult2.Value);
            Assert.IsTrue(convertResult2U.Failure);
        }

        [TestMethod]
        public void VerifyMultipleStrings()
        {
            var sourceNumber1 = 2608;
            var hexString1 = new Int64HexStringConverter($"{sourceNumber1:X}", true);
            var hexString1A = new Int64HexStringConverter($"0x{sourceNumber1:X}", true);

            var parseResults1 = hexString1.ConvertToSignedInt64();
            var parseResults1A = hexString1A.ConvertToSignedInt64();

            var parseResults1U = hexString1.ConvertToUnsignedInt64();
            var parseResults1aU = hexString1A.ConvertToUnsignedInt64();

            Assert.IsTrue(parseResults1.Success);
            Assert.AreEqual(2608, parseResults1.Value);

            Assert.IsTrue(parseResults1U.Success);
            Assert.AreEqual((ulong) 2608, parseResults1U.Value);

            Assert.IsTrue(parseResults1A.Success);
            Assert.AreEqual(2608, parseResults1A.Value);

            Assert.IsTrue(parseResults1aU.Success);
            Assert.AreEqual((ulong) 2608, parseResults1aU.Value);

            var sourceNumber2 = 13;
            var hexString2 = new Int64HexStringConverter($"{sourceNumber2:X}", true);
            var hexString2A = new Int64HexStringConverter($"0x{sourceNumber2:X}", true);
            
            var parseResults2 = hexString2.ConvertToSignedInt64();
            var parseResults2A = hexString2A.ConvertToSignedInt64();

            var parseResults2U = hexString2.ConvertToUnsignedInt64();
            var parseResults2aU = hexString2A.ConvertToUnsignedInt64();

            Assert.IsTrue(parseResults2.Success);
            Assert.AreEqual(13, parseResults2.Value);

            Assert.IsTrue(parseResults2U.Success);
            Assert.AreEqual((ulong) 13, parseResults2U.Value);

            Assert.IsTrue(parseResults2A.Success);
            Assert.AreEqual(13, parseResults2A.Value);

            Assert.IsTrue(parseResults2aU.Success);
            Assert.AreEqual((ulong) 13, parseResults2aU.Value);

            var hexString3 = new Int64HexStringConverter("-13", true);
            var parseResults3 = hexString3.ConvertToSignedInt64();
            var parseResults3U = hexString3.ConvertToUnsignedInt64();

            Assert.IsTrue(parseResults3.Failure);
            Assert.IsTrue(parseResults3U.Failure);

            var hexString4 = new Int64HexStringConverter("GAD", true);           
            var parseResults4 = hexString4.ConvertToSignedInt64();
            var parseResults4U = hexString4.ConvertToUnsignedInt64();

            Assert.IsTrue(parseResults4.Failure);
            Assert.IsTrue(parseResults4U.Failure);}
    }
}

