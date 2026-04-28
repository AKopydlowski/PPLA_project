using PPLA.Project.Core.Wind;
using Xunit;

namespace PPLA_project.Tests;

public class RunwayWindCalculatorTests
{
    [Fact]
    public void Calculate_ShouldDetectCrosswindLimitExceeded()
    {
        var calc = new RunwayWindCalculator();
        var result = calc.Calculate(90, 180, 20, 10);

        Assert.True(result.ExceedsCrosswindLimit);
    }
}
