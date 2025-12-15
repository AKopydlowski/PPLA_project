using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPLA_project.Core.WeightBalance
{
    public class WeightItem
    {
        public string Name { get; set; }
        public double WeightKg { get; set; }
        public double ArmMeters { get; set; }

        public double Moment => WeightKg * ArmMeters;
    }
}
