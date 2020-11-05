using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TFnsc.BinaryConverter.Tests.Models;

namespace TFnsc.BinaryConverter.Tests
{
    [TestClass]
    public class BinaryConverterTests
    {
        private readonly BinaryConverter<Coordinate> _coordinateConverter;

        public BinaryConverterTests()
        {
            _coordinateConverter = new BinaryConverter<Coordinate>();
        }

        [TestMethod]
        public void StringConversion()
        {
            var stringConverter = new BinaryConverter<string>();

            var original = "Téste\n\r\0";

            var converted = stringConverter.ConvertToByteArray(original);

            var convertedBack = stringConverter.ConvertFromByteArray(converted);

            Assert.AreEqual(original, convertedBack);
        }

        [TestMethod]
        public void StringListConversion()
        {
            var stringListConverter = new BinaryConverter<List<string>>();

            var original = new List<string>{
                "Téste\n\r\0",
                "123",
                "abcdE",
                "$*!Q&$#RFH#HQ3h(H#f84f4w[]`}{^{]\"'\\/"
            };

            var converted = stringListConverter.ConvertToByteArray(original);

            var convertedBack = stringListConverter.ConvertFromByteArray(converted);

            Assert.IsTrue(original.SequenceEqual(convertedBack));
        }

        [TestMethod]
        public void PrimitiveListConversion()
        {
            var stringListConverter = new BinaryConverter<int[]>();

            var original = Enumerable.Range(0, 100).ToArray();

            var converted = stringListConverter.ConvertToByteArray(original);

            var convertedBack = stringListConverter.ConvertFromByteArray(converted);

            Assert.IsTrue(original.SequenceEqual(convertedBack));
        }

        [TestMethod]
        public void ByteEnumListConversion()
        {
            var stringListConverter = new BinaryConverter<List<PlaceType>>();

            var original = new List<PlaceType> {
                PlaceType.Industry,
                PlaceType.House,
                PlaceType.ApartmentComplex,
                PlaceType.Industry
            };

            var converted = stringListConverter.ConvertToByteArray(original);

            var convertedBack = stringListConverter.ConvertFromByteArray(converted);

            Assert.IsTrue(original.SequenceEqual(convertedBack));
        }

        [TestMethod]
        public void MultilevelClassConversion()
        {
            var google = new Address
            {
                Street = "Amphitheatre Pkwy",
                City = "Mountain View",
                State = "California",
                Country = "USA",
                Number = 1600,
                Coordinate = new Coordinate
                {
                    Lat = 37.421666f,
                    Long = -122.085679f,
                    Searched = DateTime.Now,
                    AccuracyPercentage = 100,
                    Id = -14384
                },
                Type = PlaceType.Industry
            };

            var converter = new BinaryConverter<Address>();

            var converted = converter.ConvertToByteArray(google);

            var convertedBack = converter.ConvertFromByteArray(converted);

            Assert.IsTrue(google.Equals(convertedBack));
        }

        [TestMethod]
        public void ClassConversion()
        {
            var coordinate = new Coordinate
            {
                AccuracyPercentage = 166,
                Id = 5,
                Lat = 1.5652f,
                Long = 4.1526f,
                Searched = new DateTime(2000,1,1)
            };

            var bytes = _coordinateConverter.ConvertToByteArray(coordinate);
            var expected = new byte[] { 5, 0, 0, 0, 121, 88, 200, 63, 25, 226, 132, 64, 0, 64, 228, 71, 2, 34, 193, 8, 166 };

            Assert.IsTrue(expected.SequenceEqual(bytes));

            var back = _coordinateConverter.ConvertFromByteArray(expected);

            Assert.IsTrue(back.Equals(coordinate));
        }
    }
}
