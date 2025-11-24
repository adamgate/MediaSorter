using MediaSorter.Utils;
using System.Diagnostics.CodeAnalysis;

namespace MediaSorterTests.Utils
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StringUtilsTests
    {
        [TestMethod]
        [DataRow("test", "TEST", true)]
        [DataRow("test", "tEsT", true)]
        [DataRow("test", "sett", false)]
        [DataRow("", "", true)]
        [DataRow("", " ", false)]
        [DataRow(" café", " CAFÉ", true)]
        [DataRow("ß", "ss", false)]
        [DataRow("straße", "STRASSE", false)]
        [DataRow("résumé", "RÉSUMÉ", true)]
        [DataRow("123", "123", true)]
        [DataRow("123", "124", false)]
        [DataRow("hello-world", "HELLO-WORLD", true)]
        [DataRow("hello_world", "HELLO-WORLD", false)]
        [DataRow("Case", "case ", false)]
        [DataRow(null, null, true)]
        [DataRow(null, "text", false)]
        [DataRow("text", null, false)]
        public void EqualsIgnoreCase_ReturnsExpected(string val1, string val2, bool expectedResult)
        {
            // Arrange

            // Act
            var result = val1.EqualsIgnoreCase(val2);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        [DataRow(new string[] { "apple", "banana", "cherry" }, "BANANA", true)]
        [DataRow(new string[] { "apple", "banana", "cherry" }, "bAnAnA", true)]
        [DataRow(new string[] { "apple", "BANANA", "cherry" }, "banana", true)]
        [DataRow(new string[] { "apple", "banana", "cherry" }, "berry", false)]
        [DataRow(new string[] { "One", "Two", "Three" }, "one", true)]
        [DataRow(new string[] { "One", "Two", "Three" }, "four", false)]
        [DataRow(new string[] { "" }, "", true)]
        [DataRow(new string[] { "", " " }, " ", true)]
        [DataRow(new string[] { "", " " }, "", true)]
        [DataRow(new string[] { "apple", "banana", "cherry" }, "", false)]
        [DataRow(new string[] { "HELLO", "world" }, "hello", true)]
        [DataRow(new string[] { "HELLO", "world" }, "WORLD ", false)]
        [DataRow(new string[] { " café", "CAFÉ" }, "café", true)]
        [DataRow(new string[] { "café", "CAFÉ" }, "CAFÉ", true)]
        [DataRow(new string[] { "test", null }, null, true)]
        [DataRow(new string[] { "test", null }, "test", true)]
        [DataRow(new string[] { null, null }, null, true)]
        [DataRow(new string[] { }, "anything", false)]
        [DataRow(new string[] { "apple", "banana" }, null, false)]
        [DataRow(new string[] { "apple", "banana", "BANANA" }, "banana", true)]
        [DataRow(new string[] { "hello-world", "HELLO_WORLD" }, "hello-world", true)]
        public void ContainsIgnoreCase_ReturnsExpected(IEnumerable<string> source, string value, bool expectedResult)
        {
            // Arrange

            // Act
            var result = source.ContainsIgnoreCase(value);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void ContainsIgnoreCase_NullEnumerable_ThrowsException()
        {
            // Arrange
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            List<string> input = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            // Act and Assert
#pragma warning disable CS8604 // Possible null reference argument.
            Assert.ThrowsException<ArgumentNullException>(() => input.ContainsIgnoreCase("test"));
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}