using PPLA.Project.Core.Vfr;
using Xunit;

namespace PPLA_project.Tests;

public class VfrLegCalculatorTests
{
    [Fact]
    public void Calculate_ShouldReturnPositiveGroundSpeedAndTime()
    {
        var calc = new VfrLegCalculator();
        var result = calc.Calculate(new VfrLegInput
        {
            TrueCourseDeg = 90,
            DistanceNm = 60,
            TrueAirspeedKt = 100,
            WindDirectionDeg = 180,
            WindSpeedKt = 15,
            MagneticVariationDeg = 3
        });

        Assert.True(result.GroundSpeedKt > 0);
        Assert.True(result.TimeMinutes > 0);
    }
}
