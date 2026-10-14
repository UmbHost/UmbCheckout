using UmbCheckout.Shared.Persistence;
using Xunit;

namespace UmbCheckout.Tests.Persistence;

/// <summary>Quality Phase 5 review: a read that started before a save's clear must not put the old row back afterwards.</summary>
public sealed class SettingsCacheTests
{
    private sealed record Row(string Value);

    [Fact]
    public void A_read_that_started_before_a_clear_is_not_cached()
    {
        var cache = new SettingsCache<Row>();
        var generation = cache.Generation; // the reader starts its query

        cache.Clear(); // a save commits and clears
        cache.TrySet(new Row("old"), generation); // the reader finishes with the row it read before the save

        Assert.False(cache.TryGet(out _));
    }

    [Fact]
    public void A_read_with_no_clear_in_between_is_cached()
    {
        var cache = new SettingsCache<Row>();

        cache.TrySet(new Row("current"), cache.Generation);

        Assert.True(cache.TryGet(out var row));
        Assert.Equal("current", row.Required().Value);
    }

    [Fact]
    public void None_saved_yet_is_cached_too()
    {
        var cache = new SettingsCache<Row>();

        cache.TrySet(null, cache.Generation);

        Assert.True(cache.TryGet(out var row));
        Assert.Null(row);
    }
}
