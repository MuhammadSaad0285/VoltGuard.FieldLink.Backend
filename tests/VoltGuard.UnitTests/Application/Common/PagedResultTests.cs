using VoltGuard.Application.Common;

namespace VoltGuard.UnitTests.Application.Common;

public class PagedResultTests
{
    [Fact]
    public void Constructor_DefaultsItemsToEmptyCollection()
    {
        var result = new PagedResult<string>();

        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void Properties_CanStorePagingMetadataAndItems()
    {
        var result = new PagedResult<string>
        {
            Items = new[] { "A", "B" },
            PageNumber = 2,
            PageSize = 10,
            TotalCount = 25
        };

        Assert.Equal(new[] { "A", "B" }, result.Items);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalCount);
    }
}
