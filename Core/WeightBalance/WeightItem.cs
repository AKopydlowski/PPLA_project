using System;

namespace PPLA.Project.Core.WeightBalance
{
    public class WeightItem
    {
        public string Name { get; set; } = string.Empty;
        public double WeightKg { get; set; }
        public double ArmMeters { get; set; }

        public double Moment => WeightKg * ArmMeters;
    }
}
