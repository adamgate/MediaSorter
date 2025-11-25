using MediaSorter.Models;
using MediaSorter.Services.Implementations;
using System.Diagnostics.CodeAnalysis;

namespace MediaSorterTests.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DateParserTests
    {
        private DateParser _sut = new();

        [TestMethod]
        public void Parse_NoMetadata_ReturnsDefault()
        {
            // Arrange 
            var rawMetadataCollection = new List<RawMetadata>();
            var input = new Dictionary<string, IEnumerable<RawMetadata>> { { "test", rawMetadataCollection } };

            // Act
            var result = _sut.Parse(input);

            // Assert
            Assert.AreEqual(DateTime.MinValue, result.First().Value.DateTaken);
        }

        private static IEnumerable<object[]> ParseMetadataTypeData
        {
            get
            {
                return
                [
                    [new RawMetadata("Exif", "Date", "2025:10:01 00:00:00"), new DateTime(2025, 10, 1)],
                    [new RawMetadata("File", "Date", "Wed Oct 01 00:00:00 -00:00 2025"), new DateTime(2025, 9, 30, 18, 0, 0)],
                    [new RawMetadata("GPS", "Date", "2025:10:01"), new DateTime(2025, 10, 1)],
                    [new RawMetadata("ICC", "Date", "2025:10:01 00:00:00"), new DateTime(2025, 10, 1)],
                    [new RawMetadata("IPTC", "Date", "10/01/2025"), new DateTime(2025, 10, 1)],
                    [new RawMetadata("QuickTime", "Date", "Wed Oct 01 00:00:00 2025"), new DateTime(2025, 10, 1)],
                    [new RawMetadata("Unknown", "Date", "2025/10/01"), new DateTime(2025, 10, 1)]
                ];
            }
        }

        [TestMethod]
        [DynamicData(nameof(ParseMetadataTypeData))]
        public void Parse_MetadataType_ReturnsExpected(RawMetadata rawMetadata, DateTime expectedDate)
        {
            // Arrange 
            var rawMetadataCollection = new List<RawMetadata>() { rawMetadata };
            var input = new Dictionary<string, IEnumerable<RawMetadata>> { { "test",  rawMetadataCollection } };

            // Act
            var result = _sut.Parse(input);

            // Assert
            Assert.AreEqual(expectedDate, result.First().Value.DateTaken, rawMetadata.Directory);
        }
    }
}
