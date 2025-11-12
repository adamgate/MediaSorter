using AutoFixture;
using MediaSorter;
using MediaSorter.Services.Interfaces;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace MediaSorterTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public sealed class AppTests
    {
        private App _sut;
        private Mock<IDirectoryProvider> _directoryProvider;
        private Mock<IFileSorter> _fileSorter;
        private Mock<IMediaScanner> _mediaScanner;
        private Mock<IMetadataProvider> _metadataProvider;
        private readonly Fixture _fixture;

        [TestInitialize]
        public void Setup()
        {
            _directoryProvider = new();
            _fileSorter = new();
            _mediaScanner = new();
            _metadataProvider = new();
            _sut = new(
                _directoryProvider.Object, 
                _fileSorter.Object, 
                _mediaScanner.Object, 
                _metadataProvider.Object);
        }

        [TestMethod]
        public void Run_NullSourceDirectory_ReturnsSuccess()
        {
            // Arrange
            _directoryProvider
                .Setup(x => x.GetValidDirectory(_fixture.Create<string>()))
                .Returns<string?>(null);

            // Act
            //var result = _sut.Run(new string[]());

            // Assert
            // ensure that _mediaScanner.GetMediaInPath() is not hit
        }

        [TestMethod]
        public void Run_NullOutputDirectory_ReturnsSuccess()
        {
            // Assert
            // ensure that _fileSorter.SortMediaFilesByDate() is not hit
        }

        [TestMethod]
        public void Run_NonexistantOutputDirectory_ReturnsSuccess()
        {
            // Assert
            // ensure that _fileSorter.SortMediaFilesByDate() is not hit
        }

        [TestMethod]
        public void Run_NoFilesFound_ReturnsSuccess()
        {
            // Assert
            // ensure that _metadataProvider.EvaluateMediaMetadata() is not hit
        }

        [TestMethod]
        public void Run_ExceptionThrown_ReturnsSuccess()
        { }

        [TestMethod]
        public void Run_FilesSorted_ReturnsSuccess()
        { }
    }
}
