using System;
using PPLA.Project.Core.WeightBalance;
using Xunit;

namespace PPLA_project.Tests;

public class WeightBalanceCalculatorTests
{
    [Fact]
    public void CenterOfGravity_ShouldThrow_WhenTotalWeightNotPositive()
    {
        var calc = new WeightBalanceCalculator();
        Assert.Throws<InvalidOperationException>(() => calc.CenterOfGravity());
    }
}
