using System;

namespace PPLA.Project.Core.FuelPlanning
{
    public class FuelPlannerCalculator
    {
        public FuelPlanResult Calculate(FuelPlanInput input)
        {
            if (input.DistanceNm <= 0) throw new ArgumentException("Distance must be greater than 0 NM.");
            if (input.TrueAirspeedKt <= 0) throw new ArgumentException("TAS must be greater than 0 kt.");
            if (input.FuelBurnPerHourL <= 0) throw new ArgumentException("Fuel burn must be greater than 0 L/h.");

            var groundSpeed = input.TrueAirspeedKt + input.WindComponentKt;
            if (groundSpeed <= 0) throw new ArgumentException("Ground speed must be greater than 0 kt.");

            var timeHours = input.DistanceNm / groundSpeed;
            var tripFuel = timeHours * input.FuelBurnPerHourL;
            var contingencyFuel = tripFuel * (input.ContingencyPercent / 100.0);
            var blockFuel = tripFuel + contingencyFuel + input.TaxiFuelL + input.ReserveFuelL;

            return new FuelPlanResult
            {
                GroundSpeedKt = groundSpeed,
                FlightTimeHours = timeHours,
                TripFuelL = tripFuel,
                ContingencyFuelL = contingencyFuel,
                BlockFuelL = blockFuel,
                HasReserveMargin = blockFuel > (tripFuel + input.TaxiFuelL)
            };
        }
    }
}
