using System;

namespace PPLA.Project.Core.Wind
{
    public class RunwayWindCalculator
    {
        public WindComponentsResult Calculate(int runwayHeadingDeg, int windDirectionDeg, double windSpeedKt, double crosswindLimitKt)
        {
            if (windSpeedKt < 0) throw new ArgumentException("Wind speed cannot be negative.");
            if (crosswindLimitKt < 0) throw new ArgumentException("Crosswind limit cannot be negative.");

            var runway = Normalize360(runwayHeadingDeg);
            var windFrom = Normalize360(windDirectionDeg);

            var angleDeg = Normalize180(windFrom - runway);
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

        private static int Normalize360(int angleDeg)
        {
            var normalized = angleDeg % 360;
            return normalized < 0 ? normalized + 360 : normalized;
        }

        private static double Normalize180(int angleDeg)
        {
            var normalized = angleDeg % 360;
            if (normalized > 180) normalized -= 360;
            if (normalized < -180) normalized += 360;
            return normalized;
        }
    }
}
