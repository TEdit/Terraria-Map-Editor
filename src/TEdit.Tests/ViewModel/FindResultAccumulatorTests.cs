using Shouldly;
using TEdit.ViewModel;
using Xunit;

namespace TEdit.Terraria.Tests.ViewModel;

public class FindResultAccumulatorTests
{
    [Fact]
    public void Add_CountsEveryMatchWhileLimitingDisplayedResults()
    {
        var accumulator = new FindResultAccumulator(1000);
        int displayed = 0;

        for (int i = 0; i < 1500; i++)
        {
            if (accumulator.Add())
                displayed++;
        }

        accumulator.TotalCount.ShouldBe(1500);
        displayed.ShouldBe(1000);
    }
}
