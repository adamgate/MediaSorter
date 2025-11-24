using MediaSorter.Utils;
using System.Diagnostics.CodeAnalysis;

namespace MediaSorterTests.Utils
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CliUtilsTests
    {
        private const string _displayMessage = "Display Message";

        [TestMethod, Timeout(1000)]
        [DataRow("1")]
        [DataRow("2")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("text")]
        public void GetYesNoFromUser_CommandNotRecognized_Fails(string userInput)
        {
            // Arrange
            Console.SetIn(new StringReader(userInput));

            // Act and Assert
            Assert.ThrowsException<TimeoutException>(()
                => CliUtils.GetYesNoFromUser(_displayMessage));
        }

        [TestMethod]
        [DataRow("Yes")]
        [DataRow("yes")]
        [DataRow("yEs")]
        [DataRow("Y")]
        [DataRow("y")]
        public void GetYesNoFromUser_ConfirmationCommands_ReturnsTrue(string userInput)
        {
            // Arrange
            Console.SetIn(new StringReader("yEs"));

            // Act
            var result = CliUtils.GetYesNoFromUser(_displayMessage);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow("No")]
        [DataRow("no")]
        [DataRow("nO")]
        [DataRow("N")]
        [DataRow("n")]
        public void GetYesNoFromUser_DeclineCommands_ReturnsFalse(string userInput)
        {
            // Arrange
            Console.SetIn(new StringReader("userInput"));

            // Act
            var result = CliUtils.GetYesNoFromUser(_displayMessage);

            // Assert
            Assert.IsFalse(result);
        }
    }
}