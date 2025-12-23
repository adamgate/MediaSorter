using System.Diagnostics.CodeAnalysis;

using MediaSorter.Services.Implementations;
using MediaSorter.Services.Interfaces;

using Microsoft.Extensions.Logging;

using Moq;

namespace MediaSorterTests.Integration
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MediaSorterIntegrationTests
    {
        private string _testSourceDirectory;
        private string _testOutputDirectory;
        private IMediaScanner _mediaScanner;
        private IMetadataProvider _metadataProvider;
        private IDateParser _dateParser;
        private IFileSorter _fileSorter;
        private Mock<ILogger<DateParser>> _dateParserLogger;
        private Mock<ILogger<MetadataProvider>> _metadataProviderLogger;
        private Mock<ILogger<FileSorter>> _fileSorterLogger;

        [TestInitialize]
        public void Setup()
        {
            _testSourceDirectory = Path.Combine(Path.GetTempPath(), $"MediaSorterTest_Source_{Guid.NewGuid()}");
            _testOutputDirectory = Path.Combine(Path.GetTempPath(), $"MediaSorterTest_Output_{Guid.NewGuid()}");

            Directory.CreateDirectory(_testSourceDirectory);
            Directory.CreateDirectory(_testOutputDirectory);

            _mediaScanner = new MediaScanner();
            _dateParserLogger = new Mock<ILogger<DateParser>>();
            _metadataProviderLogger = new Mock<ILogger<MetadataProvider>>();
            _fileSorterLogger = new Mock<ILogger<FileSorter>>();
            _metadataProvider = new MetadataProvider(_metadataProviderLogger.Object);
            _dateParser = new DateParser(_dateParserLogger.Object);
            _fileSorter = new FileSorter(_fileSorterLogger.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testSourceDirectory))
            {
                Directory.Delete(_testSourceDirectory, true);
            }

            if (Directory.Exists(_testOutputDirectory))
            {
                Directory.Delete(_testOutputDirectory, true);
            }
        }

        [TestMethod]
        public void EndToEnd_MediaWithValidMetadata_SortsCorrectly()
        {
            // Arrange
            var testFiles = CreateTestMediaFiles();

            // Act - Scan
            var mediaPaths = _mediaScanner.GetMediaInPath(_testSourceDirectory);

            // Act - Extract Metadata
            var mediaWithMetadata = _metadataProvider.EvaluateMediaMetadata(mediaPaths);

            // Act - Parse Dates
            var mediaWithDates = _dateParser.Parse(mediaWithMetadata);

            // Act - Sort Files
            var sortResults = _fileSorter.SortMediaFilesByDate(_testOutputDirectory, mediaWithDates).ToList();

            // Assert
            Assert.AreEqual(testFiles.Count, mediaPaths.Count(), "Should scan all test files");
            Assert.AreEqual(testFiles.Count, mediaWithMetadata.Count, "Should extract metadata for all files");
            Assert.AreEqual(testFiles.Count, mediaWithDates.Count, "Should parse dates for all files");
            Assert.AreEqual(testFiles.Count, sortResults.Count, "Should sort all files");
        }

        [TestMethod]
        public void EndToEnd_MediaWithoutMetadata_SortsToUnknownFolder()
        {
            // Arrange
            CreateTestFileWithoutMetadata("no_metadata.txt", "test.jpg");

            // Act
            var mediaPaths = _mediaScanner.GetMediaInPath(_testSourceDirectory);
            var mediaWithMetadata = _metadataProvider.EvaluateMediaMetadata(mediaPaths);
            var mediaWithDates = _dateParser.Parse(mediaWithMetadata);
            var sortResults = _fileSorter.SortMediaFilesByDate(_testOutputDirectory, mediaWithDates).ToList();

            // Assert
            Assert.AreEqual(1, sortResults.Count);
            var unknownFolder = Path.Combine(_testOutputDirectory, "unknown");
            Assert.IsTrue(Directory.Exists(unknownFolder), "Unknown folder should be created");
            Assert.IsTrue(File.Exists(Path.Combine(unknownFolder, "test.jpg")), "File should be in unknown folder");
        }

        [TestMethod]
        public void EndToEnd_MixedMediaFormats_ScansOnlySupportedFormats()
        {
            // Arrange
            CreateTestFile("photo.jpg");
            CreateTestFile("video.mp4");
            CreateTestFile("document.txt");
            CreateTestFile("spreadsheet.xlsx");
            CreateTestFile("image.png");

            // Act
            var mediaPaths = _mediaScanner.GetMediaInPath(_testSourceDirectory);

            // Assert
            Assert.AreEqual(3, mediaPaths.Count(), "Should only scan supported media formats");
            Assert.IsTrue(mediaPaths.Any(p => p.EndsWith("photo.jpg")));
            Assert.IsTrue(mediaPaths.Any(p => p.EndsWith("video.mp4")));
            Assert.IsTrue(mediaPaths.Any(p => p.EndsWith("image.png")));
            Assert.IsFalse(mediaPaths.Any(p => p.EndsWith("document.txt")));
            Assert.IsFalse(mediaPaths.Any(p => p.EndsWith("spreadsheet.xlsx")));
        }

        [TestMethod]
        public void EndToEnd_NestedDirectories_ScansRecursively()
        {
            // Arrange
            var subDir1 = Path.Combine(_testSourceDirectory, "folder1");
            var subDir2 = Path.Combine(_testSourceDirectory, "folder2");
            var nestedDir = Path.Combine(subDir1, "nested");

            Directory.CreateDirectory(subDir1);
            Directory.CreateDirectory(subDir2);
            Directory.CreateDirectory(nestedDir);

            CreateTestFileInDirectory(_testSourceDirectory, "root.jpg");
            CreateTestFileInDirectory(subDir1, "sub1.png");
            CreateTestFileInDirectory(subDir2, "sub2.mp4");
            CreateTestFileInDirectory(nestedDir, "nested.jpg");

            // Act
            var mediaPaths = _mediaScanner.GetMediaInPath(_testSourceDirectory);

            // Assert
            Assert.AreEqual(4, mediaPaths.Count(), "Should find all media files in nested directories");
            Assert.IsTrue(mediaPaths.Any(p => p.EndsWith("root.jpg")));
            Assert.IsTrue(mediaPaths.Any(p => p.EndsWith("sub1.png")));
            Assert.IsTrue(mediaPaths.Any(p => p.EndsWith("sub2.mp4")));
            Assert.IsTrue(mediaPaths.Any(p => p.EndsWith("nested.jpg")));
        }

        [TestMethod]
        public void EndToEnd_DuplicateFileNames_HandlesGracefully()
        {
            // Arrange
            CreateTestFileWithoutMetadata("content1", "duplicate.jpg");
            var subDir = Path.Combine(_testSourceDirectory, "subfolder");
            Directory.CreateDirectory(subDir);
            CreateTestFileInDirectoryWithContent(subDir, "duplicate.jpg", "content2");

            // Act
            var mediaPaths = _mediaScanner.GetMediaInPath(_testSourceDirectory);
            var mediaWithMetadata = _metadataProvider.EvaluateMediaMetadata(mediaPaths);
            var mediaWithDates = _dateParser.Parse(mediaWithMetadata);
            var sortResults = _fileSorter.SortMediaFilesByDate(_testOutputDirectory, mediaWithDates).ToList();

            // Assert
            Assert.AreEqual(2, mediaPaths.Count(), "Should find both files with same name");
            Assert.AreEqual(2, sortResults.Count, "Should attempt to sort both files");

            // One should succeed, one should fail due to duplicate
            var successCount = sortResults.Count(r => r.Item2);
            var failCount = sortResults.Count(r => !r.Item2);

            Assert.AreEqual(1, successCount, "First file should copy successfully");
            Assert.AreEqual(1, failCount, "Second file should fail due to duplicate name");
        }

        [TestMethod]
        public void EndToEnd_EmptySourceDirectory_ReturnsEmptyResults()
        {
            // Arrange - empty directory already created

            // Act
            var mediaPaths = _mediaScanner.GetMediaInPath(_testSourceDirectory);

            // Assert
            Assert.AreEqual(0, mediaPaths.Count(), "Should return no files for empty directory");
        }

        [TestMethod]
        public void EndToEnd_CaseInsensitiveExtensions_ProcessesCorrectly()
        {
            // Arrange
            CreateTestFile("image1.JPG");
            CreateTestFile("image2.jpg");
            CreateTestFile("image3.Jpg");
            CreateTestFile("video.MP4");

            // Act
            var mediaPaths = _mediaScanner.GetMediaInPath(_testSourceDirectory);

            // Assert
            Assert.AreEqual(4, mediaPaths.Count(), "Should process files regardless of extension case");
        }

        [TestMethod]
        public void Integration_MetadataProvider_ExtractsDateInformation()
        {
            // Arrange
            var testFiles = CreateTestMediaFiles();

            // Act
            var mediaPaths = _mediaScanner.GetMediaInPath(_testSourceDirectory);
            var mediaWithMetadata = _metadataProvider.EvaluateMediaMetadata(mediaPaths);

            // Assert
            Assert.AreEqual(mediaPaths.Count(), mediaWithMetadata.Count);

            foreach (var media in mediaWithMetadata)
            {
                Assert.IsNotNull(media.Value, $"Metadata should not be null for {media.Key}");
                // Files without metadata should have empty collections
                Assert.IsNotNull(media.Value);
            }
        }

        [TestMethod]
        public void Integration_DateParser_ParsesMultipleFormats()
        {
            // Arrange
            var testMetadata = new Dictionary<string, IEnumerable<MediaSorter.Models.RawMetadata>>
            {
                {
                    "test1.jpg",
                    new List<MediaSorter.Models.RawMetadata>
                    {
                        new MediaSorter.Models.RawMetadata("Exif", "Date/Time Original", "2024:10:15 12:30:45")
                    }
                },
                {
                    "test2.jpg",
                    new List<MediaSorter.Models.RawMetadata>
                    {
                        new MediaSorter.Models.RawMetadata("GPS", "GPS Date Stamp", "2024:11:20")
                    }
                },
                {
                    "test3.jpg",
                    new List<MediaSorter.Models.RawMetadata>()
                }
            };

            // Act
            var result = _dateParser.Parse(testMetadata);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new DateTime(2024, 10, 15, 12, 30, 45), result["test1.jpg"].DateTaken);
            Assert.AreEqual(new DateTime(2024, 11, 20), result["test2.jpg"].DateTaken);
            Assert.AreEqual(DateTime.MinValue, result["test3.jpg"].DateTaken);
        }

        [TestMethod]
        public void Integration_FileSorter_CreatesCorrectFolderStructure()
        {
            // Arrange
            var mediaWithDates = new Dictionary<string, MediaSorter.Models.DateMetadata>
            {
                {
                    CreateTestFileWithPath("jan2024.jpg"),
                    new MediaSorter.Models.DateMetadata("", "", "", new DateTime(2024, 1, 15), 0.9)
                },
                {
                    CreateTestFileWithPath("dec2024.jpg"),
                    new MediaSorter.Models.DateMetadata("", "", "", new DateTime(2024, 12, 25), 0.9)
                }
            };

            // Act
            var results = _fileSorter.SortMediaFilesByDate(_testOutputDirectory, mediaWithDates).ToList();

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.Item2), "All files should copy successfully");

            var jan2024Path = Path.Combine(_testOutputDirectory, "2024", "01 January");
            var dec2024Path = Path.Combine(_testOutputDirectory, "2024", "12 December");

            Assert.IsTrue(Directory.Exists(jan2024Path), "January 2024 folder should exist");
            Assert.IsTrue(Directory.Exists(dec2024Path), "December 2024 folder should exist");
            Assert.IsTrue(File.Exists(Path.Combine(jan2024Path, "20240115_jan2024.jpg")));
            Assert.IsTrue(File.Exists(Path.Combine(dec2024Path, "20241225_dec2024.jpg")));
        }

        [TestMethod]
        public void Integration_FileSorter_PreservesOriginalFiles()
        {
            // Arrange
            var sourceFile = CreateTestFileWithPath("preserve.jpg");
            var mediaWithDates = new Dictionary<string, MediaSorter.Models.DateMetadata>
            {
                {
                    sourceFile,
                    new MediaSorter.Models.DateMetadata("", "", "", new DateTime(2024, 6, 15), 0.9)
                }
            };

            // Act
            _fileSorter.SortMediaFilesByDate(_testOutputDirectory, mediaWithDates).ToList();

            // Assert
            Assert.IsTrue(File.Exists(sourceFile), "Original file should still exist after sorting");
        }

        // Helper Methods

        private List<string> CreateTestMediaFiles()
        {
            var files = new List<string>
            {
                "image1.jpg",
                "image2.png",
                "video1.mp4",
                "image3.heic"
            };

            foreach (var file in files)
            {
                CreateTestFile(file);
            }

            return files;
        }

        private void CreateTestFile(string fileName)
        {
            var filePath = Path.Combine(_testSourceDirectory, fileName);
            File.WriteAllText(filePath, "test content");
        }

        private void CreateTestFileInDirectory(string directory, string fileName)
        {
            var filePath = Path.Combine(directory, fileName);
            File.WriteAllText(filePath, "test content");
        }

        private void CreateTestFileInDirectoryWithContent(string directory, string fileName, string content)
        {
            var filePath = Path.Combine(directory, fileName);
            File.WriteAllText(filePath, content);
        }

        private void CreateTestFileWithoutMetadata(string content, string fileName)
        {
            var filePath = Path.Combine(_testSourceDirectory, fileName);
            File.WriteAllText(filePath, content);
        }

        private string CreateTestFileWithPath(string fileName)
        {
            var filePath = Path.Combine(_testSourceDirectory, fileName);
            File.WriteAllText(filePath, "test content");
            return filePath;
        }
    }
}