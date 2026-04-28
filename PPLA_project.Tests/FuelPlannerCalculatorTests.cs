using PPLA.Project.Core.FuelPlanning;
using Xunit;

namespace PPLA_project.Tests;

public class FuelPlannerCalculatorTests
{
    [Fact]
    public void Calculate_ShouldReturnReserveOk_WhenLandingFuelAboveRequired()
    {
        var calc = new FuelPlannerCalculator();
        var result = calc.Calculate(new FuelPlanInput
        {
            DistanceNm = 120,
            TrueAirspeedKt = 100,
            WindComponentKt = -10,
            FuelBurnPerHourL = 30,
            TaxiFuelL = 3,
            ReserveFuelL = 20,
            ReserveMinutesRequired = 30,
            ContingencyPercent = 10
        });

        Assert.True(result.HasReserveMargin);
        Assert.True(result.EstimatedLandingFuelL >= result.ReserveRequiredFuelL);
    }
}
