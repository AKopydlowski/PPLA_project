using System.Net.Http.Json;
using System.Text;
using PPLA.Project.Core.FuelPlanning;
using PPLA.Project.Core.Decision;
using PPLA.Project.Core.History;
using PPLA.Project.Core.MetarV2;
using PPLA.Project.Core.Profiles;
using PPLA.Project.Infrastructure.History;
using PPLA.Project.Core.Metar;
using PPLA.Project.Core.Vfr;
using PPLA.Project.Core.WeightBalance;
using PPLA.Project.Core.Wind;
using PPLA.Project.UI.FuelPlanning;
using PPLA.Project.UI.Metar;
using PPLA.Project.UI.Vfr;
using PPLA.Project.UI.WeightBalance;
using PPLA.Project.UI.Wind;

var runConsole = args.Contains("--console");
if (runConsole)
{
    RunConsole();
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IFlightPlanHistoryStore, FileFlightPlanHistoryStore>();
var app = builder.Build();

app.MapGet("/", (HttpContext ctx) =>
{
    ctx.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
    ctx.Response.Headers.Pragma = "no-cache";
    ctx.Response.Headers.Expires = "0";
    return Results.Content(RenderHomePage(), "text/html", Encoding.UTF8);
});
app.MapGet("/favicon.ico", () => Results.NoContent());

app.MapPost("/api/fuel", (FuelPlanInput input) => Execute(() => new FuelPlannerCalculator().Calculate(input)));
app.MapPost("/api/wind", (WindInput input) => Execute(() => new RunwayWindCalculator().Calculate(input.RunwayHeadingDeg, input.WindDirectionDeg, input.WindSpeedKt, input.CrosswindLimitKt)));
app.MapPost("/api/vfr", (VfrLegInput input) => Execute(() => new VfrLegCalculator().Calculate(input)));
app.MapPost("/api/metar/parse", (MetarRawInput input) => Execute(() => new MetarParser().Parse(input.Raw)));

app.MapPost("/api/metar/v2/parse", (MetarRawInput input) => Execute(() => new MetarV2Parser().Parse(input.Raw)));

app.MapPost("/api/scenario/go-no-go", (FlightScenarioInput input) => Execute(() => new FlightScenarioCalculator().Evaluate(input)));

app.MapPost("/api/history/flight-plan", async (FlightPlanSaveInput input, IFlightPlanHistoryStore store) =>
{
    var record = new FlightPlanRecord(Guid.NewGuid().ToString("N"), DateTime.UtcNow, input.ScenarioName, input.PlannedFuelL, input.ActualFuelL, input.PlannedTimeMin, input.ActualTimeMin, input.AuditTrail);
    await store.SaveAsync(record);
    return Results.Ok(record);
});

app.MapGet("/api/history/flight-plan", async (IFlightPlanHistoryStore store) => Results.Ok(await store.GetAllAsync()));


app.MapGet("/api/metar/live/{icao}", async (string icao, IHttpClientFactory httpClientFactory, HttpContext ctx) =>
{
    ctx.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
    ctx.Response.Headers.Pragma = "no-cache";
    ctx.Response.Headers.Expires = "0";

    if (string.IsNullOrWhiteSpace(icao) || icao.Length < 4)
        return Results.BadRequest(new { error = "ICAO musi mieć minimum 4 znaki." });

    try
    {
        var station = icao.ToUpperInvariant();
        var client = httpClientFactory.CreateClient();
        var url = $"https://tgftp.nws.noaa.gov/data/observations/metar/stations/{station}.TXT";
        var payload = await client.GetStringAsync(url);

        var lines = payload.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return Results.NotFound(new { error = "Brak danych METAR dla podanego ICAO." });

        var observed = lines[0];
        var raw = lines[1];
        var parsed = new MetarParser().Parse(raw);

        var nextExpectedIssueUtc = TryResolveNextMetarIssueUtc(observed, raw);
        return Results.Ok(new { source = "NOAA/NWS TGFTP", station, observed, raw, parsed, nextExpectedIssueUtc });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Błąd pobierania METAR: {ex.Message}");
    }
});



static DateTime? TryResolveNextMetarIssueUtc(string observedUtcLine, string rawMetar)
{
    var source = string.IsNullOrWhiteSpace(rawMetar) ? observedUtcLine : rawMetar;
    if (string.IsNullOrWhiteSpace(source)) return null;

    var token = source.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .FirstOrDefault(t => t.Length == 7 && t.EndsWith("Z", StringComparison.OrdinalIgnoreCase) &&
                             int.TryParse(t[..2], out _) && int.TryParse(t.Substring(2, 2), out _) && int.TryParse(t.Substring(4, 2), out _));

    if (token is null) return null;

    var day = int.Parse(token[..2]);
    var hour = int.Parse(token.Substring(2, 2));
    var minute = int.Parse(token.Substring(4, 2));

    var now = DateTime.UtcNow;
    DateTime? issue = null;
    foreach (var monthOffset in new[] { 0, -1, 1 })
    {
        var baseDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(monthOffset);
        if (day > DateTime.DaysInMonth(baseDate.Year, baseDate.Month)) continue;

        var candidate = new DateTime(baseDate.Year, baseDate.Month, day, hour, minute, 0, DateTimeKind.Utc);
        if (issue is null || Math.Abs((candidate - now).TotalDays) < Math.Abs((issue.Value - now).TotalDays))
            issue = candidate;
    }

    if (issue is null) return null;

    var intervalMinutes = minute >= 30 ? 30 : 60;
    return issue.Value.AddMinutes(intervalMinutes);
}

app.Run();

static IResult Execute<T>(Func<T> fn)
{
    try { return Results.Ok(fn()); }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (Exception ex) { return Results.Problem(ex.Message); }
}

static string RenderHomePage() => """
<!DOCTYPE html><html lang="pl"><head><meta charset="UTF-8"/><meta name="viewport" content="width=device-width, initial-scale=1.0"/><title>PPLA Planner</title>
<style>
:root{--bg:#0b1220;--panel:#1e293b;--txt:#e2e8f0;--muted:#93a4c2;--acc:#3b82f6;--ok:#22c55e;--bad:#ef4444}
*{box-sizing:border-box}body{margin:0;font-family:Inter,Arial,sans-serif;background:var(--bg);color:var(--txt)}
.wrap{max-width:1280px;margin:24px auto;padding:0 16px}.hero{background:linear-gradient(120deg,#1d4ed8,#0f172a);padding:18px;border-radius:14px;margin-bottom:16px}
.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(290px,1fr));gap:16px}.card{background:var(--panel);padding:16px;border-radius:14px}
.sec{font-size:30px;font-weight:800}.muted{color:var(--muted)}.field{margin:.55rem 0}label{display:block;margin-bottom:4px}
input,textarea,button{width:100%;padding:10px;border-radius:10px;border:1px solid #334155;background:#020617;color:var(--txt)}button{cursor:pointer;background:var(--acc);border:none;font-weight:700}
.result{margin-top:10px;min-height:90px;padding:10px;border-radius:10px;background:#020617;white-space:pre-wrap;border-left:4px solid #334155}.result.ok{border-left-color:var(--ok)}.result.err{border-left-color:var(--bad)}
.badge{display:inline-block;font-size:12px;padding:4px 8px;border-radius:20px;background:#1d4ed8;margin-bottom:8px}
</style></head><body><div class="wrap"><div class="hero"><div class="sec">PPLA Planner</div><div class="muted">Build: 2026-04-29</div></div>
<div class="grid">
<div class="card"><span class="badge">Fuel</span><h3>Fuel Planner</h3><p class="muted">Plan paliwa i margines rezerwy.</p>
<div class="field"><label>Dystans (NM)</label><input id="fd" value="" placeholder="0"></div><div class="field"><label>TAS (kt)</label><input id="ft" value="" placeholder="0"></div><div class="field"><label>Wiatr (+/- kt)</label><input id="fw" value="" placeholder="0"></div>
<div class="field"><label>Spalanie (L/h)</label><input id="ff" value="" placeholder="0"></div><div class="field"><label>Taxi fuel (L)</label><input id="fx" value="" placeholder="0"></div><div class="field"><label>Reserve fuel (L)</label><input id="fr" value="" placeholder="0"></div>
<button onclick="fuel()">Przelicz Fuel</button><div id="fuelOut" class="result"></div></div>

<div class="card"><span class="badge">Wind</span><h3>Crosswind</h3><p class="muted">Składowe wiatru i limit boczny.</p>
<div class="field"><label>Kierunek RWY (°)</label><input id="rh" value="" placeholder="0"></div><div class="field"><label>Kierunek wiatru (°)</label><input id="wd" value="" placeholder="0"></div><div class="field"><label>Prędkość wiatru (kt)</label><input id="ws" value="" placeholder="0"></div><div class="field"><label>Limit crosswind (kt)</label><input id="cl" value="" placeholder="0"></div>
<button onclick="wind()">Przelicz Crosswind</button><div id="windOut" class="result"></div></div>

<div class="card"><span class="badge">VFR</span><h3>VFR Leg</h3><p class="muted">Heading, WCA, GS i czas odcinka.</p>
<div class="field"><label>True course (°)</label><input id="vc" value="" placeholder="0"></div><div class="field"><label>Dystans (NM)</label><input id="vd" value="" placeholder="0"></div><div class="field"><label>TAS (kt)</label><input id="vt" value="" placeholder="0"></div>
<div class="field"><label>Wind direction (°)</label><input id="vwdir" value="" placeholder="0"></div><div class="field"><label>Wind speed (kt)</label><input id="vwsp" value="" placeholder="0"></div><div class="field"><label>Mag variation (°)</label><input id="vmag" value="" placeholder="0"></div>
<button onclick="vfr()">Przelicz VFR</button><div id="vfrOut" class="result"></div></div>

<div class="card"><span class="badge">METAR</span><h3>METAR</h3><p class="muted">Live NOAA + parser ręczny.</p>
<div class="field"><label>ICAO (np. EPWA)</label><input id="icao" value="" placeholder="np. EPWA"></div><button onclick="liveMetar()">Pobierz live METAR</button><button style="margin-top:8px" onclick="toggleMetarAutoRefresh(this)">Włącz auto-odświeżanie METAR (60s)</button><div id="metarOut" class="result"></div>
<hr style="border-color:#334155;margin:12px 0"><div class="field"><label>Ręczny METAR/TAF</label><textarea id="raw" rows="4" placeholder="Wklej METAR/TAF..."></textarea></div><button onclick="parseMetar()">Parsuj ręcznie</button><div id="rawOut" class="result"></div></div>
</div></div>
<script>
async function api(url,method,body){const r=await fetch(url,{method,headers:{'Content-Type':'application/json'},body:body?JSON.stringify(body):undefined});const t=await r.text();let d={};try{d=t?JSON.parse(t):{}}catch{d={raw:t}};if(!r.ok)throw new Error(d.error||d.title||d.raw||'Błąd API');return d}
const fmt=(n,u='')=>typeof n==='number'?n.toFixed(1)+u:n;
const paint=(id,msg,err=false)=>{const el=document.getElementById(id);el.textContent=msg;el.className='result '+(err?'err':'ok')};
const num=(v)=>{const n=Number((v??'').toString().trim());return Number.isFinite(n)?n:0};
async function fuel(){try{const d=await api('/api/fuel','POST',{distanceNm:num(fd.value),trueAirspeedKt:num(ft.value),windComponentKt:num(fw.value),fuelBurnPerHourL:num(ff.value),taxiFuelL:num(fx.value),reserveFuelL:num(fr.value),contingencyPercent:5});paint('fuelOut',`GS: ${fmt(d.groundSpeedKt,' kt')}\nCzas: ${fmt(d.flightTimeHours,' h')}\nTrip: ${fmt(d.tripFuelL,' L')}\nContingency: ${fmt(d.contingencyFuelL,' L')}\nBlock: ${fmt(d.blockFuelL,' L')}\nRezerwa: ${d.hasReserveMargin?'OK':'BRAK'}`)}catch(e){paint('fuelOut',e.message,true)}}
async function wind(){try{const d=await api('/api/wind','POST',{runwayHeadingDeg:num(rh.value),windDirectionDeg:num(wd.value),windSpeedKt:num(ws.value),crosswindLimitKt:num(cl.value)});paint('windOut',`Headwind: ${fmt(d.headwindKt,' kt')}\nCrosswind: ${fmt(d.crosswindKt,' kt')}\nLimit: ${d.exceedsCrosswindLimit?'PRZEKROCZONY':'OK'}`)}catch(e){paint('windOut',e.message,true)}}
async function vfr(){try{const d=await api('/api/vfr','POST',{trueCourseDeg:num(vc.value),distanceNm:num(vd.value),trueAirspeedKt:num(vt.value),windDirectionDeg:num(vwdir.value),windSpeedKt:num(vwsp.value),magneticVariationDeg:num(vmag.value)});paint('vfrOut',`WCA: ${fmt(d.windCorrectionAngleDeg,'°')}\nTrue HDG: ${fmt(d.trueHeadingDeg,'°')}\nMag HDG: ${fmt(d.magneticHeadingDeg,'°')}\nGS: ${fmt(d.groundSpeedKt,' kt')}\nCzas: ${fmt(d.timeMinutes,' min')}`)}catch(e){paint('vfrOut',e.message,true)}}
let metarAutoRefreshId=null;
const nextIssueLabel=(iso)=>iso?new Date(iso).toLocaleString('pl-PL',{timeZone:'UTC',hour12:false})+' UTC':'brak danych';
async function liveMetar(){try{const station=(icao.value||'').trim().toUpperCase();if(!station){throw new Error('Podaj kod ICAO.')} const d=await api('/api/metar/live/'+station+'?t='+Date.now(),'GET');paint('metarOut',`Źródło: ${d.source}
Stacja: ${d.station}
Czas obserwacji: ${d.observed}
Kolejna spodziewana publikacja: ${nextIssueLabel(d.nextExpectedIssueUtc)}
Raw: ${d.raw}

Wiatr: ${d.parsed?.wind||'-'}
Widzialność: ${d.parsed?.visibility||'-'}
Chmury: ${d.parsed?.clouds||'-'}
QNH: ${d.parsed?.qnh||'-'}`)}catch(e){paint('metarOut',e.message,true)}}
function toggleMetarAutoRefresh(btn){if(metarAutoRefreshId){clearInterval(metarAutoRefreshId);metarAutoRefreshId=null;btn.textContent='Włącz auto-odświeżanie METAR (60s)';return;}liveMetar();metarAutoRefreshId=setInterval(liveMetar,60000);btn.textContent='Wyłącz auto-odświeżanie METAR';}
async function parseMetar(){try{const d=await api('/api/metar/parse','POST',{raw:raw.value});paint('rawOut',`Wiatr: ${d.wind||'-'}\nWidzialność: ${d.visibility||'-'}\nChmury: ${d.clouds||'-'}\nQNH: ${d.qnh||'-'}`)}catch(e){paint('rawOut',e.message,true)}}
</script></body></html>
""";

static void RunConsole()
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("=== PPLA Planner ===");
        Console.WriteLine("1. Weight & Balance");
        Console.WriteLine("2. Fuel & Flight Planner");
        Console.WriteLine("3. Runway Wind / Crosswind");
        Console.WriteLine("4. METAR/TAF Parser");
        Console.WriteLine("5. VFR Planner");
        Console.WriteLine("0. Exit");
        Console.Write("Choose module: ");

        var choice = Console.ReadLine();
        Console.WriteLine();

        if (choice == "0")
        {
            Console.WriteLine("Koniec. Safe flights!");
            break;
        }

        try
        {
            switch (choice)
            {
                case "1": WeightBalanceConsoleUI.Run(); break;
                case "2": FuelPlanningConsoleUI.Run(); break;
                case "3": WindConsoleUI.Run(); break;
                case "4": MetarConsoleUI.Run(); break;
                case "5": VfrConsoleUI.Run(); break;
                default: Console.WriteLine("Nieznana opcja."); break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
    }
}

record WindInput(int RunwayHeadingDeg, int WindDirectionDeg, double WindSpeedKt, double CrosswindLimitKt);
record MetarRawInput(string Raw);

record FlightPlanSaveInput(string ScenarioName, double PlannedFuelL, double ActualFuelL, double PlannedTimeMin, double ActualTimeMin, List<string> AuditTrail);

public partial class Program { }

