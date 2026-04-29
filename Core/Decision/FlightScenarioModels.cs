using PPLA.Project.Core.Profiles;
using PPLA.Project.Core.WeightBalance;

namespace PPLA.Project.Core.Decision;

public record FlightLegInput(double TrueCourseDeg, double DistanceNm, double WindDirectionDeg, double WindSpeedKt, double MagneticVariationDeg);

public record FlightScenarioInput(
    string ScenarioName,
    AircraftProfile Aircraft,
    PilotProfile Pilot,
    double TrueAirspeedKt,
    double WindDirectionDeg,
    double WindSpeedKt,
    double RunwayHeadingDeg,
    double PlannedFuelL,
    double TaxiFuelL,
    double ReserveFuelL,
    List<FlightLegInput> Legs,
    double EmptyWeightKg,
    double EmptyWeightArm,
    List<WeightItem> WeightItems);

public record FlightScenarioResult(
    bool Go,
    string Status,
    List<string> Violations,
    List<string> Recommendations,
    double TotalDistanceNm,
    double EstimatedTimeMinutes,
    double EstimatedTripFuelL,
    double CrosswindKt,
    double TotalWeightKg,
    double CgArm,
    List<string> AuditTrail);
