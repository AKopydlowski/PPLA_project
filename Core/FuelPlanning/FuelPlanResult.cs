namespace PPLA.Project.Core.FuelPlanning
{
    public class FuelPlanResult
    {
        public double GroundSpeedKt { get; set; }
        public double FlightTimeHours { get; set; }
        public double TripFuelL { get; set; }
        public double ContingencyFuelL { get; set; }
        public double BlockFuelL { get; set; }
        public bool HasReserveMargin { get; set; }
    }
}
