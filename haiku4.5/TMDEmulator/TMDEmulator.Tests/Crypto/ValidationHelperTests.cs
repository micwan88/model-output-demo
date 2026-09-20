using TMDEmulator.Main.Utilities;
using Xunit;

namespace TMDEmulator.Tests.Crypto;

public class ValidationHelperTests
{
    [Fact]
    public void IsValidDirectoryPath_WithCurrentDirectory_ReturnsTrue()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var result = ValidationHelper.IsValidDirectoryPath(currentDir);
        Assert.True(result);
    }

    [Fact]
    public void IsValidDirectoryPath_WithInvalidPath_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidDirectoryPath("/nonexistent/path/12345");
        Assert.False(result);
    }

    [Fact]
    public void IsValidDirectoryPath_WithEmptyString_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidDirectoryPath("");
        Assert.False(result);
    }

    [Fact]
    public void ValidateOutputPath_WithValidDirectory_ReturnsTrue()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var (isValid, errorMessage) = ValidationHelper.ValidateOutputPath(currentDir);
        Assert.True(isValid);
        Assert.Empty(errorMessage);
    }

    [Fact]
    public void ValidateOutputPath_WithInvalidPath_ReturnsFalse()
    {
        var (isValid, errorMessage) = ValidationHelper.ValidateOutputPath("/nonexistent/path/12345");
        Assert.False(isValid);
        Assert.NotEmpty(errorMessage);
    }

    [Fact]
    public void ExpandPath_WithCurrentDirectorySymbol_ExpandsCorrectly()
    {
        var expanded = ValidationHelper.ExpandPath("./");
        Assert.Equal(Directory.GetCurrentDirectory(), expanded);
    }

    [Fact]
    public void ExpandPath_WithEmptyPath_ReturnsCurrentDirectory()
    {
        var expanded = ValidationHelper.ExpandPath("");
        Assert.Equal(Directory.GetCurrentDirectory(), expanded);
    }
}
