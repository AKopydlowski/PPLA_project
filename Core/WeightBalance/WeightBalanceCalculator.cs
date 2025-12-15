using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPLA_project.Core.WeightBalance
{
    public class WeightBalanceCalculator
    {
        public double EmptyWeightKg { get; set; }
        public double EmptyWeightArm { get; set; }

        public List<WeightItem> Items { get; } = new();

        public void AddItem(WeightItem item)
        {
            Items.Add(item);
        }

        public double TotalWeightKg()
        {
            return EmptyWeightKg + Items.Sum(i => i.WeightKg);
        }

        public double CenterOfGravity()
        {
            double totalMoment =
                (EmptyWeightKg * EmptyWeightArm) +
                Items.Sum(i => i.Moment);

            return totalMoment / TotalWeightKg();
        }
    }
}
