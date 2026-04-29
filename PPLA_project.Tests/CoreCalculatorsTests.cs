using FluentAssertions;
using PPLA.Project.Core.FuelPlanning;
using PPLA.Project.Core.Vfr;
using PPLA.Project.Core.WeightBalance;
using PPLA.Project.Core.Wind;

public class CoreCalculatorsTests
{
    [Fact]
    public void FuelPlanner_GoldenCase_ShouldCalculateBlockFuel()
    {
        var result = new FuelPlannerCalculator().Calculate(new FuelPlanInput{DistanceNm=120,TrueAirspeedKt=100,WindComponentKt=-10,FuelBurnPerHourL=24,TaxiFuelL=3,ReserveFuelL=12,ContingencyPercent=5});
        result.BlockFuelL.Should().BeApproximately(48.6, 0.2);
    }

    [Fact]
    public void FuelPlanner_Edge_HeadwindTooStrong_ShouldThrow()
    {
        var act = () => new FuelPlannerCalculator().Calculate(new FuelPlanInput{DistanceNm=50,TrueAirspeedKt=60,WindComponentKt=-70,FuelBurnPerHourL=20});
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WindCalculator_StrongCrosswind_ShouldExceedLimit()
    {
        var result = new RunwayWindCalculator().Calculate(270, 180, 35, 15);
        result.ExceedsCrosswindLimit.Should().BeTrue();
    }

    [Fact]
    public void VfrCalculator_LowGroundSpeed_ShouldThrow()
    {
        var act = () => new VfrLegCalculator().Calculate(new VfrLegInput{DistanceNm=100,TrueCourseDeg=180,TrueAirspeedKt=70,WindDirectionDeg=180,WindSpeedKt=120,MagneticVariationDeg=4});
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WeightBalance_ShouldComputeCg()
    {
        var wb = new WeightBalanceCalculator{EmptyWeightKg=700,EmptyWeightArm=2.2};
        wb.AddItem(new WeightItem("Pilot",80,2.0));
        wb.CenterOfGravity().Should().BeGreaterThan(2.0);
    }
}
