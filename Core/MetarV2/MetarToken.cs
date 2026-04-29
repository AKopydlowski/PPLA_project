namespace PPLA.Project.Core.MetarV2;

public record MetarToken(string Value, string Type);
public record MetarIssue(string Severity, string Message);
public record MetarV2Result(string Raw, List<MetarToken> Tokens, List<MetarIssue> Issues, string PlainLanguage);
