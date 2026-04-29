namespace PPLA.Project.Core.Profiles;

public record AircraftProfile(
    string Name,
    double MaxCrosswindKt,
    double FuelBurnPerHourL,
    double MaxTakeoffWeightKg,
    double MinReserveFuelL,
    double CgMinArm,
    double CgMaxArm);
