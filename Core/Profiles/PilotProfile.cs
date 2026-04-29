namespace PPLA.Project.Core.Profiles;

public record PilotProfile(
    string Name,
    double PersonalCrosswindLimitKt,
    bool StudentMode,
    string PreferredScenario);
