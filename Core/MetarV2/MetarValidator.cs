namespace PPLA.Project.Core.MetarV2;

public class MetarValidator
{
    public List<MetarIssue> Validate(List<MetarToken> tokens)
    {
        var issues = new List<MetarIssue>();
        if (!tokens.Any(t => t.Type == "Wind")) issues.Add(new("error", "Brak grupy wiatru."));
        if (!tokens.Any(t => t.Type == "Visibility")) issues.Add(new("warning", "Brak grupy widzialności."));
        if (!tokens.Any(t => t.Type == "QNH")) issues.Add(new("warning", "Brak QNH."));
        if (tokens.Any(t => t.Type == "Unknown")) issues.Add(new("warning", "Wykryto nierozpoznane tokeny."));
        return issues;
    }
}
