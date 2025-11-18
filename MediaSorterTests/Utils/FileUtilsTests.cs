using MediaSorter.Utils;
using System.Diagnostics.CodeAnalysis;

namespace MediaSorterTests.Utils
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FileUtilsTests
    {
        [TestMethod]
        [DataRow(@"file<name", "filename")]
        [DataRow(@"inva>lid", "invalid")]
        [DataRow(@"col:on", "colon")]
        [DataRow(@"qu""ote", "quote")]
        [DataRow(@"path/segment", "pathsegment")]
        [DataRow(@"back\slash", "backslash")]
        [DataRow(@"pipe|value", "pipevalue")]
        [DataRow(@"question?mark", "questionmark")]
        [DataRow(@"star*file", "starfile")]
        [DataRow(@"mix<of>all:the""chars/here\|?", "mixofallthecharshere")]
        public void StripIllegalFileCharacters_ReturnsExpected(string input, string expected)
        {
            // Arrange

            // Act
            var result = FileUtils.StripIllegalFileCharacters(input);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}