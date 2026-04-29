using System.Text.Json;
using PPLA.Project.Core.History;

namespace PPLA.Project.Infrastructure.History;

public class FileFlightPlanHistoryStore : IFlightPlanHistoryStore
{
    private readonly string _path;

    public FileFlightPlanHistoryStore(string? path = null)
    {
        _path = path ?? Path.Combine(AppContext.BaseDirectory, "flight_history.json");
    }

    public async Task SaveAsync(FlightPlanRecord record, CancellationToken ct = default)
    {
        var all = (await GetAllAsync(ct)).ToList();
        all.Add(record);
        await using var stream = File.Create(_path);
        await JsonSerializer.SerializeAsync(stream, all, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<FlightPlanRecord>> GetAllAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_path)) return new List<FlightPlanRecord>();
        await using var stream = File.OpenRead(_path);
        var data = await JsonSerializer.DeserializeAsync<List<FlightPlanRecord>>(stream, cancellationToken: ct);
        return data ?? new List<FlightPlanRecord>();
    }
}
