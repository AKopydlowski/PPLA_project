namespace PPLA.Project.Core.History;

public record FlightPlanRecord(
    string Id,
    DateTime CreatedUtc,
    string ScenarioName,
    double PlannedFuelL,
    double ActualFuelL,
    double PlannedTimeMin,
    double ActualTimeMin,
    List<string> AuditTrail);

public interface IFlightPlanHistoryStore
{
    Task SaveAsync(FlightPlanRecord record, CancellationToken ct = default);
    Task<IReadOnlyList<FlightPlanRecord>> GetAllAsync(CancellationToken ct = default);
}
