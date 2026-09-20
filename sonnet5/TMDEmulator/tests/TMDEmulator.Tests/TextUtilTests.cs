using TMDEmulator.Ui;

namespace TMDEmulator.Tests;

public sealed class TextUtilTests
{
    [Fact]
    public void Wrap_splits_into_fixed_width_chunks_with_a_shorter_last_chunk()
    {
        Assert.Equal(["ABCD", "EFGH", "IJ"], TextUtil.Wrap("ABCDEFGHIJ", 4));
    }

    [Fact]
    public void Wrap_of_exact_multiple_has_no_empty_tail()
    {
        Assert.Equal(["ABCD", "EFGH"], TextUtil.Wrap("ABCDEFGH", 4));
    }

    [Fact]
    public void Wrap_of_short_text_is_one_chunk()
    {
        Assert.Equal(["AB"], TextUtil.Wrap("AB", 10));
    }

    [Fact]
    public void Wrap_of_empty_text_is_empty()
    {
        Assert.Empty(TextUtil.Wrap("", 4));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Wrap_with_no_room_yields_nothing_instead_of_looping_forever(int width)
    {
        Assert.Empty(TextUtil.Wrap("ABC", width));
    }

    [Fact]
    public void Fit_leaves_text_that_fits_untouched()
    {
        Assert.Equal("hello", TextUtil.Fit("hello", 5));
        Assert.Equal("hello", TextUtil.Fit("hello", 50));
    }

    [Fact]
    public void Fit_cuts_long_text_and_marks_it_with_an_ellipsis_within_the_width()
    {
        var fitted = TextUtil.Fit("hello world", 6);

        Assert.Equal("hello…", fitted);
        Assert.Equal(6, fitted.Length);
    }

    [Fact]
    public void Fit_handles_tiny_widths()
    {
        Assert.Equal("…", TextUtil.Fit("hello", 1));
        Assert.Equal("", TextUtil.Fit("hello", 0));
        Assert.Equal("", TextUtil.Fit("hello", -1));
    }
}
