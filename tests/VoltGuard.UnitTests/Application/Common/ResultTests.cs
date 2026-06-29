using VoltGuard.Application.Common;

namespace VoltGuard.UnitTests.Application.Common;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessfulResultWithoutError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_ReturnsFailedResultWithError()
    {
        var result = Result.Failure("Something went wrong.");

        Assert.False(result.IsSuccess);
        Assert.Equal("Something went wrong.", result.Error);
    }
}
