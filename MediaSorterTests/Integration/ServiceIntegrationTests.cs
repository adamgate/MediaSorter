using System.Diagnostics.CodeAnalysis;

using MediaSorter.Services.Implementations;

using Microsoft.Extensions.Logging;

using Moq;

namespace MediaSorterTests.Integration
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ServiceIntegrationTests
    {
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"ServiceIntegrationTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [TestMethod]
        public void MediaScanner_And_MetadataProvider_WorkTogether()
        {
            // Arrange
            CreateJpegFile("test1.jpg");
            CreateJpegFile("test2.jpg");
            CreateTextFile("readme.txt");

            var scanner = new MediaScanner();
            var provider = new MetadataProvider(new Mock<ILogger<MetadataProvider>>().Object);

            // Act
            var mediaPaths = scanner.GetMediaInPath(_testDirectory);
            var metadata = provider.EvaluateMediaMetadata(mediaPaths);

            // Assert
            Assert.AreEqual(2, mediaPaths.Count(), "Scanner should find 2 JPEG files");
            Assert.AreEqual(2, metadata.Count, "Metadata provider should process 2 files");
            Assert.IsFalse(mediaPaths.Any(p => p.EndsWith("readme.txt")), "Scanner should exclude .txt files");
        }

        [TestMethod]
        public void MetadataProvider_And_DateParser_WorkTogether()
        {
            // Arrange
            CreateJpegFile("photo.jpg");

            var provider = new MetadataProvider(new Mock<ILogger<MetadataProvider>>().Object);
            var parser = new DateParser(new Mock<ILogger<DateParser>>().Object);
            var scanner = new MediaScanner();

            // Act
            var mediaPaths = scanner.GetMediaInPath(_testDirectory);
            var metadata = provider.EvaluateMediaMetadata(mediaPaths);
            var dates = parser.Parse(metadata);

            // Assert
            Assert.AreEqual(1, dates.Count);
            Assert.IsNotNull(dates.First().Value);
            Assert.IsTrue(dates.ContainsKey(mediaPaths.First()));
        }

        [TestMethod]
        public void FullPipeline_ScanExtractParseSort_WorksEndToEnd()
        {
            // Arrange
            var outputDir = Path.Combine(Path.GetTempPath(), $"Output_{Guid.NewGuid()}");
            Directory.CreateDirectory(outputDir);

            try
            {
                CreateJpegFile("vacation.jpg");
                CreateJpegFile("party.jpg");

                var scanner = new MediaScanner();
                var provider = new MetadataProvider(new Mock<ILogger<MetadataProvider>>().Object);
                var parser = new DateParser(new Mock<ILogger<DateParser>>().Object);
                var sorter = new FileSorter(new Mock<ILogger<FileSorter>>().Object);

                // Act
                var step1_scan = scanner.GetMediaInPath(_testDirectory);
                var step2_metadata = provider.EvaluateMediaMetadata(step1_scan);
                var step3_dates = parser.Parse(step2_metadata);
                var step4_sort = sorter.SortMediaFilesByDate(outputDir, step3_dates).ToList();

                // Assert
                Assert.AreEqual(2, step1_scan.Count(), "Step 1: Scan should find 2 files");
                Assert.AreEqual(2, step2_metadata.Count, "Step 2: Metadata extraction should process 2 files");
                Assert.AreEqual(2, step3_dates.Count, "Step 3: Date parsing should handle 2 files");
                Assert.AreEqual(2, step4_sort.Count, "Step 4: Sorting should process 2 files");

                // Verify files were copied
                var unknownDir = Path.Combine(outputDir, "unknown");
                Assert.IsTrue(Directory.Exists(unknownDir), "Unknown folder should be created for files without metadata");
            }
            finally
            {
                if (Directory.Exists(outputDir))
                {
                    Directory.Delete(outputDir, true);
                }
            }
        }

        private void CreateJpegFile(string fileName)
        {
            var filePath = Path.Combine(_testDirectory, fileName);
            // Create minimal valid JPEG file
            byte[] jpegHeader = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
            byte[] jpegFooter = { 0xFF, 0xD9 };

            using (var fs = File.Create(filePath))
            {
                fs.Write(jpegHeader, 0, jpegHeader.Length);
                fs.Write(jpegFooter, 0, jpegFooter.Length);
            }
        }

        private void CreateTextFile(string fileName)
        {
            var filePath = Path.Combine(_testDirectory, fileName);
            File.WriteAllText(filePath, "This is a text file");
        }
    }
}