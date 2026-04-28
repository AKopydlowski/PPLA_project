using System;

namespace PPLA.Project.Core.Vfr
{
    public class VfrLegCalculator
    {
        public VfrLegResult Calculate(VfrLegInput input)
        {
            if (input.DistanceNm <= 0) throw new ArgumentException("Distance must be greater than 0 NM.");
            if (input.TrueAirspeedKt <= 0) throw new ArgumentException("TAS must be greater than 0 kt.");

            var windAngleRad = ToRadians(input.WindDirectionDeg - input.TrueCourseDeg);
            var crosswind = input.WindSpeedKt * Math.Sin(windAngleRad);
            var headwind = input.WindSpeedKt * Math.Cos(windAngleRad);

            var wcaRad = Math.Asin(Clamp(crosswind / input.TrueAirspeedKt, -1, 1));
            var headingTrue = Normalize(input.TrueCourseDeg + ToDegrees(wcaRad));
            var gs = input.TrueAirspeedKt * Math.Cos(wcaRad) - headwind;
            if (gs <= 0) throw new ArgumentException("Ground speed must be greater than 0 kt.");

            var timeMinutes = (input.DistanceNm / gs) * 60.0;

            return new VfrLegResult
            {
                WindCorrectionAngleDeg = ToDegrees(wcaRad),
                TrueHeadingDeg = headingTrue,
                MagneticHeadingDeg = Normalize(headingTrue - input.MagneticVariationDeg),
                GroundSpeedKt = gs,
                TimeMinutes = timeMinutes
            };
        }

        private static double ToRadians(double d) => d * Math.PI / 180.0;
        private static double ToDegrees(double r) => r * 180.0 / Math.PI;

        private static double Normalize(double deg)
        {
            var n = deg % 360;
            return n < 0 ? n + 360 : n;
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
