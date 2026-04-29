using PPLA.Project.Core.FuelPlanning;
using PPLA.Project.Core.Profiles;
using PPLA.Project.Core.Vfr;
using PPLA.Project.Core.WeightBalance;
using PPLA.Project.Core.Wind;

namespace PPLA.Project.Core.Decision;

public class FlightScenarioCalculator
{
    public FlightScenarioResult Evaluate(FlightScenarioInput input)
    {
        if (input.Legs.Count == 0) throw new ArgumentException("Scenario musi zawierać co najmniej 1 leg.");

        var audit = new List<string>();
        var violations = new List<string>();
        var recommendations = new List<string>();

        var wind = new RunwayWindCalculator().Calculate((int)input.RunwayHeadingDeg, (int)input.WindDirectionDeg, input.WindSpeedKt, input.Aircraft.MaxCrosswindKt);
        var crosswindAbs = Math.Abs(wind.CrosswindKt);
        audit.Add($"Crosswind: {crosswindAbs:F1} kt");

        var effectiveCrosswindLimit = Math.Min(input.Aircraft.MaxCrosswindKt, input.Pilot.PersonalCrosswindLimitKt);
        if (crosswindAbs > effectiveCrosswindLimit)
        {
            violations.Add($"Crosswind {crosswindAbs:F1} kt > limit {effectiveCrosswindLimit:F1} kt");
            recommendations.Add("Rozważ inną RWY lub przesunięcie ETA, aby zmniejszyć wiatr boczny.");
        }

        var totalDistance = input.Legs.Sum(l => l.DistanceNm);
        var totalTimeMinutes = 0.0;
        foreach (var leg in input.Legs)
        {
            var legResult = new VfrLegCalculator().Calculate(new VfrLegInput
            {
                TrueCourseDeg = leg.TrueCourseDeg,
                DistanceNm = leg.DistanceNm,
                TrueAirspeedKt = input.TrueAirspeedKt,
                WindDirectionDeg = leg.WindDirectionDeg,
                WindSpeedKt = leg.WindSpeedKt,
                MagneticVariationDeg = leg.MagneticVariationDeg
            });
            totalTimeMinutes += legResult.TimeMinutes;
        }
        audit.Add($"Total distance: {totalDistance:F1} NM");
        audit.Add($"Estimated time: {totalTimeMinutes:F1} min");

        var fuel = new FuelPlannerCalculator().Calculate(new FuelPlanInput
        {
            DistanceNm = totalDistance,
            TrueAirspeedKt = input.TrueAirspeedKt,
            WindComponentKt = -Math.Abs(wind.HeadwindKt) / 2.0,
            FuelBurnPerHourL = input.Aircraft.FuelBurnPerHourL,
            TaxiFuelL = input.TaxiFuelL,
            ReserveFuelL = input.ReserveFuelL,
            ContingencyPercent = 5
        });
        audit.Add($"Estimated block fuel: {fuel.BlockFuelL:F1} L");

        if (input.PlannedFuelL < fuel.BlockFuelL)
        {
            violations.Add($"Planned fuel {input.PlannedFuelL:F1} L < required block fuel {fuel.BlockFuelL:F1} L");
            recommendations.Add("Zwiększ planned fuel lub skróć trasę.");
        }

        if (input.ReserveFuelL < input.Aircraft.MinReserveFuelL)
        {
            violations.Add($"Reserve fuel {input.ReserveFuelL:F1} L poniżej minimum profilu {input.Aircraft.MinReserveFuelL:F1} L");
            recommendations.Add("Zwiększ reserve fuel zgodnie z profilem samolotu.");
        }

        var wb = new WeightBalanceCalculator
        {
            EmptyWeightKg = input.EmptyWeightKg,
            EmptyWeightArm = input.EmptyWeightArm
        };
        foreach (var item in input.WeightItems) wb.AddItem(item);

        var totalWeight = wb.TotalWeightKg();
        var cg = wb.CenterOfGravity();
        audit.Add($"Weight: {totalWeight:F1} kg, CG: {cg:F2}");

        if (totalWeight > input.Aircraft.MaxTakeoffWeightKg)
        {
            violations.Add($"MTOW exceeded: {totalWeight:F1} kg > {input.Aircraft.MaxTakeoffWeightKg:F1} kg");
            recommendations.Add("Usuń ładunek/paliwo lub zmień konfigurację lotu.");
        }

        if (cg < input.Aircraft.CgMinArm || cg > input.Aircraft.CgMaxArm)
        {
            violations.Add($"CG poza envelope: {cg:F2} (zakres {input.Aircraft.CgMinArm:F2}-{input.Aircraft.CgMaxArm:F2})");
            recommendations.Add("Przesuń masę (pasażer/bagaż) aby wrócić do envelope CG.");
        }

        var go = violations.Count == 0;
        var status = go ? "GO" : "NO-GO";
        if (!go && input.Pilot.StudentMode)
            recommendations.Add("Skonsultuj plan z instruktorem przed wykonaniem lotu.");

        return new FlightScenarioResult(go, status, violations, recommendations, totalDistance, totalTimeMinutes, fuel.TripFuelL, crosswindAbs, totalWeight, cg, audit);
    }
}
