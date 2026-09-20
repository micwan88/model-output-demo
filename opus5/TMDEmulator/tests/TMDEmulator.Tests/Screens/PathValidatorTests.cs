using TMDEmulator.Screens;
using TMDEmulator.Tests.Support;

namespace TMDEmulator.Tests.Screens;

public sealed class PathValidatorTests : IDisposable
{
    private readonly TempDirectory _temp = new();

    public void Dispose() => _temp.Dispose();

    [Fact]
    public void ExistingAbsoluteDirectory_IsValid()
    {
        PathValidationResult result = PathValidator.ValidateDirectory(_temp.Path, "/unused");

        Assert.True(result.IsValid);
        Assert.Equal(Path.GetFullPath(_temp.Path), result.FullPath);
    }

    [Fact]
    public void RelativeDirectory_ResolvesAgainstCurrentDirectory()
    {
        Directory.CreateDirectory(_temp.Combine("sub"));

        PathValidationResult result = PathValidator.ValidateDirectory("  sub  ", _temp.Path);

        Assert.True(result.IsValid);
        Assert.Equal(_temp.Combine("sub"), result.FullPath);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_IsRejected(string input)
    {
        PathValidationResult result = PathValidator.ValidateDirectory(input, _temp.Path);

        Assert.False(result.IsValid);
        Assert.Equal("Output path is required.", result.Error);
    }

    [Theory]
    [InlineData("out<put")]
    [InlineData("out>put")]
    [InlineData("\"quoted\"")]
    [InlineData("a|b")]
    [InlineData("a?b")]
    [InlineData("a*b")]
    [InlineData("a\tb")]
    [InlineData("a\0b")]
    public void InvalidCharacters_AreRejected(string input)
    {
        PathValidationResult result = PathValidator.ValidateDirectory(input, _temp.Path);

        Assert.False(result.IsValid);
        Assert.StartsWith("Output path contains an invalid character", result.Error);
    }

    [Fact]
    public void File_IsRejected()
    {
        string file = _temp.Combine("file.txt");
        File.WriteAllText(file, "x");

        PathValidationResult result = PathValidator.ValidateDirectory(file, _temp.Path);

        Assert.Equal($"Output path is a file, not a directory: {file}", result.Error);
    }

    [Fact]
    public void MissingDirectory_IsRejectedAndNotCreated()
    {
        string missing = _temp.Combine("missing");

        PathValidationResult result = PathValidator.ValidateDirectory(missing, _temp.Path);

        Assert.Equal($"Directory does not exist: {missing}", result.Error);
        Assert.False(Directory.Exists(missing));
    }

    [Fact]
    public void PathThatCannotBeResolved_IsRejected()
    {
        // Path.GetFullPath rejects a base directory that is not fully qualified.
        PathValidationResult result = PathValidator.ValidateDirectory("sub", "relative-base");

        Assert.False(result.IsValid);
        Assert.StartsWith("Output path is invalid:", result.Error);
    }
}
