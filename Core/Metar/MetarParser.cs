using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PPLA.Project.Core.Metar
{
    public class MetarParser
    {
        private static readonly Regex QnhRegex = new("^(Q\\d{4}|A\\d{4})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex WindRegex = new("^(\\d{3}|VRB)\\d{2,3}(G\\d{2,3})?KT$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex VisibilityRegex = new("^(\\d{4}|\\d{1,2}SM)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CloudRegex = new("^(FEW|SCT|BKN|OVC)\\d{3}(CB|TCU)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public MetarReport Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new ArgumentException("METAR/TAF input is empty.");
            }

            var tokens = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var report = new MetarReport { Raw = raw.Trim() };

            report.Wind = tokens.FirstOrDefault(t => WindRegex.IsMatch(t));
            report.Visibility = tokens.FirstOrDefault(t => VisibilityRegex.IsMatch(t));
            report.Clouds = tokens.FirstOrDefault(t => CloudRegex.IsMatch(t));
            report.Qnh = tokens.FirstOrDefault(t => QnhRegex.IsMatch(t));

            return report;
        }
    }
}
