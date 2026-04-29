namespace PPLA.Project.Core.MetarV2;

public class MetarV2Parser
{
    private readonly MetarTokenizer _tokenizer = new();
    private readonly MetarValidator _validator = new();

    public MetarV2Result Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) throw new ArgumentException("METAR/TAF input is empty.");

        var tokens = _tokenizer.Tokenize(raw.Trim());
        var issues = _validator.Validate(tokens);

        var wind = tokens.FirstOrDefault(t => t.Type == "Wind")?.Value ?? "brak";
        var vis = tokens.FirstOrDefault(t => t.Type == "Visibility")?.Value ?? "brak";
        var cloud = tokens.FirstOrDefault(t => t.Type == "Cloud")?.Value ?? "brak";
        var qnh = tokens.FirstOrDefault(t => t.Type == "QNH")?.Value ?? "brak";

        var plain = $"Raport: wiatr {wind}, widzialność {vis}, chmury {cloud}, ciśnienie {qnh}.";
        return new MetarV2Result(raw.Trim(), tokens, issues, plain);
    }
}
