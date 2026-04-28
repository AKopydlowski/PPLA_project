using System;

namespace PPLA.Project.Core.Wind
{
    public class RunwayWindCalculator
    {
        public WindComponentsResult Calculate(int runwayHeadingDeg, int windDirectionDeg, double windSpeedKt, double crosswindLimitKt)
        {
            if (windSpeedKt < 0) throw new ArgumentException("Wind speed cannot be negative.");

            var angleDeg = NormalizeAngle(windDirectionDeg - runwayHeadingDeg);
            var angleRad = angleDeg * Math.PI / 180.0;

            var headwind = Math.Cos(angleRad) * windSpeedKt;
            var crosswind = Math.Sin(angleRad) * windSpeedKt;

            return new WindComponentsResult
            {
                HeadwindKt = headwind,
                CrosswindKt = crosswind,
                ExceedsCrosswindLimit = Math.Abs(crosswind) > crosswindLimitKt
            };
        }

        private static double NormalizeAngle(int angleDeg)
        {
            var normalized = angleDeg % 360;
            if (normalized > 180) normalized -= 360;
            if (normalized < -180) normalized += 360;
            return normalized;
        }
    }
}
