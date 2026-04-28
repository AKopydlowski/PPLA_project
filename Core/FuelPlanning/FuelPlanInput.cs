namespace PPLA.Project.Core.FuelPlanning
{
    public class FuelPlanInput
    {
        public double DistanceNm { get; set; }
        public double TrueAirspeedKt { get; set; }
        public double WindComponentKt { get; set; }
        public double FuelBurnPerHourL { get; set; }
        public double TaxiFuelL { get; set; }
        public double ReserveFuelL { get; set; }
        public double ContingencyPercent { get; set; } = 5;
    }
}
