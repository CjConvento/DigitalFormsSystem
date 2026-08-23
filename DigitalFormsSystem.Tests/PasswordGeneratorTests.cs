using Xunit;
using DigitalFormsSystem.Core.Helpers;

namespace DigitalFormsSystem.Tests
{
    public class PasswordGeneratorTests
    {
        // --- Tests for GenerateRandomString ---

        [Fact]
        public void GenerateRandomString_ReturnsCorrectLength()
        {
            // Arrange
            int length = 10;

            // Act
            var result = PasswordGenerator.GenerateRandomString(length);

            // Assert
            Assert.Equal(length, result.Length);
        }

        [Fact]
        public void GenerateRandomString_BelowMinimumLength_ClampsToFour()
        {
            // Arrange & Act
            // The method forces a minimum length of 4, even if you ask for less
            var result = PasswordGenerator.GenerateRandomString(2);

            // Assert
            Assert.Equal(4, result.Length);
        }

        [Fact]
        public void GenerateRandomString_TwoConsecutiveCalls_ReturnDifferentValues()
        {
            // Arrange & Act
            var result1 = PasswordGenerator.GenerateRandomString(12);
            var result2 = PasswordGenerator.GenerateRandomString(12);

            // Assert
            // Statistically, two independently random 12-character strings
            // should not be equal. If this ever fails, something is wrong
            // with the randomness source.
            Assert.NotEqual(result1, result2);
        }

        [Fact]
        public void GenerateRandomString_ContainsAtLeastOneOfEachCharacterType()
        {
            // Arrange & Act
            var result = PasswordGenerator.GenerateRandomString(12);

            // Assert
            Assert.Contains(result, char.IsUpper);
            Assert.Contains(result, char.IsLower);
            Assert.Contains(result, char.IsDigit);
        }

        // --- Tests for GeneratePasswordWithEmployeeNo ---

        [Fact]
        public void GeneratePasswordWithEmployeeNo_ValidEmployeeNo_StartsWithPrefix()
        {
            // Arrange
            string employeeNo = "HS9501-0019";

            // Act
            var result = PasswordGenerator.GeneratePasswordWithEmployeeNo(employeeNo, 8);

            // Assert
            Assert.StartsWith("HS9501@", result);
        }

        [Fact]
        public void GeneratePasswordWithEmployeeNo_LongPrefix_TruncatesToSixCharacters()
        {
            // Arrange
            // "CE150622" (before the dash) is longer than 6 characters,
            // so the prefix portion should be cut down to 6.
            string employeeNo = "CE150622-2238";

            // Act
            var result = PasswordGenerator.GeneratePasswordWithEmployeeNo(employeeNo, 8);

            // Assert
            Assert.StartsWith("CE1506@", result);
        }

        [Fact]
        public void GeneratePasswordWithEmployeeNo_NoDashInEmployeeNo_UsesWholeStringAsPrefix()
        {
            // Arrange
            // Some employee numbers might not follow the XX0000-0000 format.
            // This test documents what currently happens if there's no dash.
            string employeeNo = "TEST11";

            // Act
            var result = PasswordGenerator.GeneratePasswordWithEmployeeNo(employeeNo, 8);

            // Assert
            Assert.StartsWith("TEST11@", result);
        }

        [Fact]
        public void GeneratePasswordWithEmployeeNo_TwoConsecutiveCalls_ReturnDifferentPasswords()
        {
            // Arrange
            string employeeNo = "HS9501-0019";

            // Act
            var password1 = PasswordGenerator.GeneratePasswordWithEmployeeNo(employeeNo, 8);
            var password2 = PasswordGenerator.GeneratePasswordWithEmployeeNo(employeeNo, 8);

            // Assert
            // Same employee number, but the random portion should differ each time.
            Assert.NotEqual(password1, password2);
        }
    }
}
