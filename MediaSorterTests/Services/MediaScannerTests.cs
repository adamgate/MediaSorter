using System.Diagnostics.CodeAnalysis;

using MediaSorter.Services.Implementations;

namespace MediaSorterTests.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MediaScannerTests
    {
        private MediaScanner _sut;
        private string _testDirectory;

        [TestMethod]
        public void GetMediaInPath_SupportedFormats_ReturnsMedia()
        {
            // Arrange
            var testFiles = new[] { "test.jpg", "test.png", "test.mp4", "test.mov" };
            CreateTestFiles(testFiles);

            // Act
            var result = _sut.GetMediaInPath(_testDirectory);

            // Assert
            Assert.AreEqual(4, result.Count());
            foreach (var file in testFiles)
            {
                Assert.IsTrue(result.Any(r => r.EndsWith(file)));
            }
        }

        [TestMethod]
        public void GetMediaInPath_UnsupportedFormats_ExcludesFiles()
        {
            // Arrange
            var testFiles = new[] { "test.txt", "test.docx", "test.pdf" };
            CreateTestFiles(testFiles);

            // Act
            var result = _sut.GetMediaInPath(_testDirectory);

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GetMediaInPath_MixedFormats_ReturnsOnlySupported()
        {
            // Arrange
            var testFiles = new[] { "photo.jpg", "document.txt", "video.mp4", "readme.md", "image.heic" };
            CreateTestFiles(testFiles);

            // Act
            var result = _sut.GetMediaInPath(_testDirectory);

            // Assert
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(result.Any(r => r.EndsWith("photo.jpg")));
            Assert.IsTrue(result.Any(r => r.EndsWith("video.mp4")));
            Assert.IsTrue(result.Any(r => r.EndsWith("image.heic")));
        }

        [TestMethod]
        public void GetMediaInPath_Subdirectories_ReturnsAllMedia()
        {
            // Arrange
            var subDir = Path.Combine(_testDirectory, "subfolder");
            Directory.CreateDirectory(subDir);

            CreateTestFile("root.jpg");
            File.Create(Path.Combine(subDir, "sub.png")).Close();

            // Act
            var result = _sut.GetMediaInPath(_testDirectory);

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(r => r.EndsWith("root.jpg")));
            Assert.IsTrue(result.Any(r => r.EndsWith("sub.png")));
        }

        [TestMethod]
        public void GetMediaInPath_EmptyDirectory_ReturnsEmpty()
        {
            // Arrange - directory already created in TestInitialize

            // Act
            var result = _sut.GetMediaInPath(_testDirectory);

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GetMediaInPath_CaseInsensitiveExtensions_ReturnsMedia()
        {
            // Arrange
            var testFiles = new[] { "test.JPG", "test.PNG", "test.Mp4", "test.MOV" };
            CreateTestFiles(testFiles);

            // Act
            var result = _sut.GetMediaInPath(_testDirectory);

            // Assert
            Assert.AreEqual(4, result.Count());
        }

        [TestInitialize]
        public void Setup()
        {
            _sut = new MediaScanner();
            _testDirectory = Path.Combine(Path.GetTempPath(), $"MediaSorterTest_{Guid.NewGuid()}");
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

        private void CreateTestFiles(string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                CreateTestFile(fileName);
            }
        }

        private void CreateTestFile(string fileName)
        {
            var filePath = Path.Combine(_testDirectory, fileName);
            File.Create(filePath).Close();
        }
    }
}