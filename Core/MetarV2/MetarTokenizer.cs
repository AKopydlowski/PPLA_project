using System.Text.RegularExpressions;

namespace PPLA.Project.Core.MetarV2;

public class MetarTokenizer
{
    private static readonly Regex Wind = new("^(\\d{3}|VRB)\\d{2,3}(G\\d{2,3})?KT$", RegexOptions.IgnoreCase);
    private static readonly Regex Qnh = new("^(Q\\d{4}|A\\d{4})$", RegexOptions.IgnoreCase);
    private static readonly Regex Cloud = new("^(FEW|SCT|BKN|OVC)\\d{3}(CB|TCU)?$", RegexOptions.IgnoreCase);
    private static readonly Regex Vis = new("^(\\d{4}|\\d{1,2}SM|CAVOK)$", RegexOptions.IgnoreCase);

    public List<MetarToken> Tokenize(string raw)
    {
        var split = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return split.Select(t => new MetarToken(t, Classify(t))).ToList();
    }

    private static string Classify(string token)
    {
        if (Wind.IsMatch(token)) return "Wind";
        if (Qnh.IsMatch(token)) return "QNH";
        if (Cloud.IsMatch(token)) return "Cloud";
        if (Vis.IsMatch(token)) return "Visibility";
        if (token.EndsWith("Z") && token.Length == 7) return "Time";
        if (token is "METAR" or "TAF") return "Header";
        return "Unknown";
    }
}
