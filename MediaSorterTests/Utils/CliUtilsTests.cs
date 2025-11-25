using MediaSorter.Utils;
using System.Diagnostics.CodeAnalysis;

namespace MediaSorterTests.Utils
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CliUtilsTests
    {
        private const string _displayMessage = "Display Message";
        private const int _timeoutInMs = 500;

        [TestMethod]
        [DataRow("1")]
        [DataRow("2")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("text")]
        public void GetYesNoFromUser_CommandNotRecognized_Fails(string userInput)
        {
            var task = Task.Run(() =>
            {
                // Arrange
                Console.SetIn(new StringReader(userInput));

                // Act
                var result = CliUtils.GetYesNoFromUser(_displayMessage);

                // Assert
                Assert.Fail("Test didn't time out.");
            });

            if (!task.Wait(_timeoutInMs))
                Assert.IsTrue(true, "Test timed out as expected.");
        }

        [TestMethod]
        [DataRow("Yes")]
        public void GetYesNoFromUser_ConfirmationCommands_ReturnsTrue(string userInput)
        {
            // Arrange
            Console.SetIn(new StringReader(userInput));

            // Act
            var result = CliUtils.GetYesNoFromUser(_displayMessage);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow("No")]
        public void GetYesNoFromUser_DeclineCommands_ReturnsFalse(string userInput)
        {
            // Arrange
            Console.SetIn(new StringReader(userInput));

            // Act
            var result = CliUtils.GetYesNoFromUser(_displayMessage);

            // Assert
            Assert.IsFalse(result);
        }
    }
}