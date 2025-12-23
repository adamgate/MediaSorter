using System.Diagnostics.CodeAnalysis;

using MediaSorter.Models;
using MediaSorter.Services.Implementations;

using Microsoft.Extensions.Logging;

using Moq;

namespace MediaSorterTests.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DateParserTests
    {
        private Mock<ILogger<DateParser>> _logger;
        private DateParser _sut;

        private static IEnumerable<object[]> ParseMetadataTypeData
        {
            get
            {
                return
                [
                    [new RawMetadata("Exif", "Date/Time Original", "2025:10:01 00:00:00"), new DateTime(2025, 10, 1), 0.9],
                    [new RawMetadata("QuickTime", "Created", "Wed Oct 01 00:00:00 2025"), new DateTime(2025, 10, 1), 0.9],
                    [new RawMetadata("GPS", "GPS Date Stamp", "2025:10:01"), new DateTime(2025, 10, 1), 0.8],
                    [new RawMetadata("Exif", "Date/Time Digitized", "2025:10:01 00:00:00"), new DateTime(2025, 10, 1), 0.7],
                    [new RawMetadata("Exif", "Date/Time", "2025:10:01 00:00:00"), new DateTime(2025, 10, 1), 0.6],
                    [new RawMetadata("File", "File Modified Date", "Wed Oct 01 00:00:00 -00:00 2025"), new DateTime(2025, 9, 30, 18, 0, 0), 0.0],
                    [new RawMetadata("ICC", "Date", "2025:10:01 00:00:00"), new DateTime(2025, 10, 1), 0.1],
                    [new RawMetadata("IPTC", "Date", "10/01/2025"), new DateTime(2025, 10, 1), 0.1],
                    [new RawMetadata("Unknown", "Date", "2025/10/01"), new DateTime(2025, 10, 1), 0.1]
                ];
            }
        }

        [TestMethod]
        [DynamicData(nameof(ParseMetadataTypeData))]
        public void Parse_MetadataType_ReturnsExpectedDateAndWeight(RawMetadata rawMetadata, DateTime expectedDate, double expectedWeight)
        {
            // Arrange
            var rawMetadataCollection = new List<RawMetadata>() { rawMetadata };
            var input = new Dictionary<string, IEnumerable<RawMetadata>> { { "test", rawMetadataCollection } };

            // Act
            var result = _sut.Parse(input);

            // Assert
            Assert.AreEqual(expectedDate, result.First().Value.DateTaken, $"Date mismatch for {rawMetadata.Directory}");
            Assert.AreEqual(expectedWeight, result.First().Value.AccuracyWeight, $"Weight mismatch for {rawMetadata.Name}");
        }

        [TestMethod]
        public void Parse_MultipleMetadataEntries_SelectsMostAccurate()
        {
            // Arrange
            var rawMetadataCollection = new List<RawMetadata>
            {
                new RawMetadata("File", "File Modified Date", "Wed Oct 01 00:00:00 -00:00 2025"), // Weight 0.0
                new RawMetadata("Exif", "Date/Time", "2025:10:05 00:00:00"), // Weight 0.6
                new RawMetadata("Exif", "Date/Time Original", "2025:10:10 00:00:00") // Weight 0.9 - should win
            };
            var input = new Dictionary<string, IEnumerable<RawMetadata>> { { "test", rawMetadataCollection } };

            // Act
            var result = _sut.Parse(input);

            // Assert
            Assert.AreEqual(new DateTime(2025, 10, 10), result.First().Value.DateTaken);
            Assert.AreEqual(0.9, result.First().Value.AccuracyWeight);
            Assert.AreEqual("Date/Time Original", result.First().Value.Name);
        }

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
            Assert.AreEqual(0, result.First().Value.AccuracyWeight);
        }

        [TestMethod]
        public void Parse_InvalidDateFormat_ReturnsMinValue()
        {
            // Arrange
            var rawMetadataCollection = new List<RawMetadata>
            {
                new RawMetadata("Exif", "Date/Time Original", "invalid date")
            };
            var input = new Dictionary<string, IEnumerable<RawMetadata>> { { "test", rawMetadataCollection } };

            // Act
            var result = _sut.Parse(input);

            // Assert
            Assert.AreEqual(DateTime.MinValue, result.First().Value.DateTaken);
        }

        [TestMethod]
        public void Parse_MultipleMediaFiles_ProcessesAll()
        {
            // Arrange
            var input = new Dictionary<string, IEnumerable<RawMetadata>>
            {
                {
                    "file1.jpg",
                    new List<RawMetadata> { new RawMetadata("Exif", "Date/Time Original", "2025:10:01 00:00:00") }
                },
                {
                    "file2.jpg",
                    new List<RawMetadata> { new RawMetadata("Exif", "Date/Time Original", "2025:10:15 00:00:00") }
                },
                {
                    "file3.jpg",
                    new List<RawMetadata>() // No metadata
                }
            };

            // Act
            var result = _sut.Parse(input);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new DateTime(2025, 10, 1), result["file1.jpg"].DateTaken);
            Assert.AreEqual(new DateTime(2025, 10, 15), result["file2.jpg"].DateTaken);
            Assert.AreEqual(DateTime.MinValue, result["file3.jpg"].DateTaken);
        }

        [TestInitialize]
        public void Setup()
        {
            _logger = new();
            _sut = new(_logger.Object);
        }
    }
}