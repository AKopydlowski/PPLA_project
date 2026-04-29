using System.Net.Http.Json;
using System.Text;
using PPLA.Project.Core.FuelPlanning;
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
var app = builder.Build();

app.MapGet("/", () => Results.Content(RenderHomePage(), "text/html", Encoding.UTF8));

app.MapPost("/api/fuel", (FuelPlanInput input) =>
{
    var calc = new FuelPlannerCalculator();
    return Results.Ok(calc.Calculate(input));
});

app.MapPost("/api/wind", (WindInput input) =>
{
    var calc = new RunwayWindCalculator();
    return Results.Ok(calc.Calculate(input.RunwayHeadingDeg, input.WindDirectionDeg, input.WindSpeedKt, input.CrosswindLimitKt));
});

app.MapPost("/api/vfr", (VfrLegInput input) =>
{
    var calc = new VfrLegCalculator();
    return Results.Ok(calc.Calculate(input));
});

app.MapPost("/api/metar/parse", (MetarRawInput input) =>
{
    var parser = new MetarParser();
    return Results.Ok(parser.Parse(input.Raw));
});

app.MapGet("/api/metar/live/{icao}", async (string icao, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
    var url = $"https://aviationweather.gov/api/data/metar?ids={icao.ToUpperInvariant()}&format=json";
    var response = await client.GetFromJsonAsync<List<AviationWeatherMetarDto>>(url);

    if (response is null || response.Count == 0)
    {
        return Results.NotFound(new { message = "Brak danych METAR dla podanego ICAO." });
    }

    var metar = response[0];
    var parser = new MetarParser();
    var parsed = parser.Parse(metar.RawOb ?? string.Empty);

    return Results.Ok(new
    {
        source = "NOAA AviationWeather.gov",
        station = metar.IcaoId,
        observed = metar.ObsTime,
        raw = metar.RawOb,
        parsed
    });
});

app.Run();

static string RenderHomePage() => """
<!DOCTYPE html>
<html lang="pl">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>PPLA Planner Web</title>
  <style>
    body { font-family: Arial, sans-serif; margin: 0; background:#0f172a; color:#e2e8f0; }
    .wrap { max-width: 1100px; margin: 24px auto; padding: 0 16px; }
    .grid { display:grid; grid-template-columns: repeat(auto-fill,minmax(250px,1fr)); gap:16px; }
    .card { background:#1e293b; border-radius:14px; padding:16px; box-shadow: 0 8px 24px rgba(0,0,0,.25); }
    input, button { width:100%; margin-top:8px; padding:10px; border-radius:8px; border:1px solid #334155; background:#0b1220; color:#e2e8f0; }
    button { background:#2563eb; border:none; cursor:pointer; }
    pre { white-space: pre-wrap; background:#020617; padding:10px; border-radius:8px; }
  </style>
</head>
<body>
<div class="wrap">
  <h1>PPLA Planner – Web UI</h1>
  <p>Dashboard modułów + live METAR z oficjalnego źródła AviationWeather (NOAA).</p>
  <div class="grid">
    <div class="card">
      <h3>Fuel Planner</h3>
      <input id="fd" placeholder="Distance NM" value="120" />
      <input id="ft" placeholder="TAS kt" value="105" />
      <input id="fw" placeholder="Wind component kt" value="-10" />
      <input id="ff" placeholder="Burn L/h" value="24" />
      <input id="fx" placeholder="Taxi L" value="3" />
      <input id="fr" placeholder="Reserve L" value="12" />
      <button onclick="fuel()">Przelicz</button>
      <pre id="fuelOut"></pre>
    </div>
    <div class="card">
      <h3>Crosswind</h3>
      <input id="rh" placeholder="RWY heading" value="270" />
      <input id="wd" placeholder="Wind direction" value="230" />
      <input id="ws" placeholder="Wind speed kt" value="18" />
      <input id="cl" placeholder="Crosswind limit kt" value="15" />
      <button onclick="wind()">Przelicz</button>
      <pre id="windOut"></pre>
    </div>
    <div class="card">
      <h3>VFR leg</h3>
      <input id="vc" placeholder="True course" value="210" />
      <input id="vd" placeholder="Distance NM" value="75" />
      <input id="vt" placeholder="TAS kt" value="102" />
      <input id="vwdir" placeholder="Wind dir" value="250" />
      <input id="vwsp" placeholder="Wind speed" value="20" />
      <input id="vmag" placeholder="Mag var" value="4" />
      <button onclick="vfr()">Przelicz</button>
      <pre id="vfrOut"></pre>
    </div>
    <div class="card">
      <h3>Live METAR</h3>
      <input id="icao" placeholder="ICAO" value="EPWA" />
      <button onclick="liveMetar()">Pobierz</button>
      <pre id="metarOut"></pre>
    </div>
  </div>
</div>
<script>
async function post(url, body){ const r = await fetch(url,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(body)}); return r.json(); }
const show=(id,obj)=>document.getElementById(id).textContent = JSON.stringify(obj,null,2);
async function fuel(){ show('fuelOut', await post('/api/fuel',{distanceNm:+fd.value,trueAirspeedKt:+ft.value,windComponentKt:+fw.value,fuelBurnPerHourL:+ff.value,taxiFuelL:+fx.value,reserveFuelL:+fr.value,contingencyPercent:5})); }
async function wind(){ show('windOut', await post('/api/wind',{runwayHeadingDeg:+rh.value,windDirectionDeg:+wd.value,windSpeedKt:+ws.value,crosswindLimitKt:+cl.value})); }
async function vfr(){ show('vfrOut', await post('/api/vfr',{trueCourseDeg:+vc.value,distanceNm:+vd.value,trueAirspeedKt:+vt.value,windDirectionDeg:+vwdir.value,windSpeedKt:+vwsp.value,magneticVariationDeg:+vmag.value})); }
async function liveMetar(){ const r = await fetch('/api/metar/live/'+icao.value); show('metarOut', await r.json()); }
</script>
</body>
</html>
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
record AviationWeatherMetarDto(string? IcaoId, string? RawOb, string? ObsTime);
