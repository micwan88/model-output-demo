using System.Text;
using TMDEmulator.Export;
using TMDEmulator.Keys;
using TMDEmulator.Tests.Support;

namespace TMDEmulator.Tests;

public sealed class MzmkInitExporterTests : IDisposable
{
    private static readonly DateTimeOffset Utc = new(2026, 9, 20, 10, 5, 7, TimeSpan.Zero);
    private static readonly EcKeyInfo RealKey = EcKeyInspector.Inspect(TestKeyPair.Pkcs8Pem);

    private readonly TempDirectory _dir = new();

    public void Dispose() => _dir.Dispose();

    private static MzmkInitExporter Exporter(TimeZoneInfo? zone = null) =>
        new(RealKey, new FixedTimeProvider(Utc, zone));

    [Fact]
    public void File_name_is_fingerprint_then_timestamp_then_der()
    {
        Assert.Equal("MZMK_Init_11D3BE2920260920100507.der", Exporter().BuildFileName());
    }

    [Fact]
    public void File_name_uses_local_time_not_utc()
    {
        var plus8 = TimeZoneInfo.CreateCustomTimeZone("plus8", TimeSpan.FromHours(8), "plus8", "plus8");

        Assert.Equal("MZMK_Init_11D3BE2920260920180507.der", Exporter(plus8).BuildFileName());
    }

    [Fact]
    public void Timestamp_is_zero_padded_24_hour()
    {
        var early = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var exporter = new MzmkInitExporter(RealKey, new FixedTimeProvider(early));

        Assert.Equal("MZMK_Init_11D3BE2920260102030405.der", exporter.BuildFileName());
    }

    [Fact]
    public void Plan_for_an_existing_folder_targets_a_file_inside_it()
    {
        Assert.True(Exporter().TryPlan(_dir.Path, out var target, out var error));

        Assert.Equal(string.Empty, error);
        Assert.Equal(_dir.Combine("MZMK_Init_11D3BE2920260920100507.der"), target.FullPath);
        Assert.False(target.Exists);
    }

    [Fact]
    public void Plan_trims_surrounding_whitespace()
    {
        Assert.True(Exporter().TryPlan($"  {_dir.Path}  ", out var target, out _));

        Assert.Equal(_dir.Path, Path.GetDirectoryName(target.FullPath));
    }

    [Fact]
    public void Plan_resolves_a_relative_folder_to_an_absolute_path()
    {
        var relative = Path.GetRelativePath(Directory.GetCurrentDirectory(), _dir.Path);

        Assert.True(Exporter().TryPlan(relative, out var target, out _));

        Assert.True(Path.IsPathRooted(target.FullPath));
        Assert.Equal(_dir.Path, Path.GetDirectoryName(target.FullPath));
    }

    [Fact]
    public void Plan_reports_when_the_file_already_exists()
    {
        File.WriteAllText(_dir.Combine("MZMK_Init_11D3BE2920260920100507.der"), "old");

        Assert.True(Exporter().TryPlan(_dir.Path, out var target, out _));

        Assert.True(target.Exists);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Plan_rejects_an_empty_folder(string input)
    {
        Assert.False(Exporter().TryPlan(input, out _, out var error));

        Assert.Contains("must not be empty", error);
    }

    [Fact]
    public void Plan_rejects_a_folder_that_does_not_exist_and_does_not_create_it()
    {
        var missing = _dir.Combine("missing");

        Assert.False(Exporter().TryPlan(missing, out _, out var error));

        Assert.Contains("Folder does not exist", error);
        Assert.Contains(missing, error);
        Assert.False(Directory.Exists(missing));
    }

    [Fact]
    public void Plan_rejects_a_path_with_illegal_characters()
    {
        Assert.False(Exporter().TryPlan(_dir.Path + "\0evil", out _, out var error));

        Assert.Contains("Invalid folder path", error);
    }

    [Fact]
    public void Plan_rejects_a_path_that_is_a_file()
    {
        var file = _dir.Combine("afile");
        File.WriteAllText(file, "x");

        Assert.False(Exporter().TryPlan(file, out _, out var error));

        Assert.Contains("Folder does not exist", error);
    }

    [Fact]
    public void Write_saves_exactly_the_public_point_hex_as_plain_ascii()
    {
        var exporter = Exporter();
        exporter.TryPlan(_dir.Path, out var target, out _);

        exporter.Write(target.FullPath, overwrite: false);

        var bytes = File.ReadAllBytes(target.FullPath);
        Assert.Equal(Encoding.ASCII.GetBytes(ReferenceKey.PublicPointHex), bytes); // no BOM, no newline
        Assert.EndsWith(".der", target.FullPath);
    }

    [Fact]
    public void Write_without_overwrite_refuses_to_replace_an_existing_file()
    {
        var path = _dir.Combine("existing.der");
        File.WriteAllText(path, "keep me");

        Assert.ThrowsAny<IOException>(() => Exporter().Write(path, overwrite: false));

        Assert.Equal("keep me", File.ReadAllText(path));
    }

    [Fact]
    public void Write_with_overwrite_replaces_the_whole_file()
    {
        var path = _dir.Combine("existing.der");
        File.WriteAllText(path, new string('X', 500)); // longer than the new content: must be truncated

        Exporter().Write(path, overwrite: true);

        Assert.Equal(ReferenceKey.PublicPointHex, File.ReadAllText(path));
    }

    [Fact]
    public void Write_into_a_missing_folder_throws_an_io_exception()
    {
        Assert.ThrowsAny<IOException>(() => Exporter().Write(_dir.Combine("nope/x.der"), overwrite: false));
    }

    [Fact]
    public void Exporter_exposes_the_key_it_exports()
    {
        Assert.Same(RealKey, Exporter().Key);
    }
}
