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

app.MapPost("/api/fuel", (FuelPlanInput input) => Execute(() => new FuelPlannerCalculator().Calculate(input)));
app.MapPost("/api/wind", (WindInput input) => Execute(() => new RunwayWindCalculator().Calculate(input.RunwayHeadingDeg, input.WindDirectionDeg, input.WindSpeedKt, input.CrosswindLimitKt)));
app.MapPost("/api/vfr", (VfrLegInput input) => Execute(() => new VfrLegCalculator().Calculate(input)));
app.MapPost("/api/metar/parse", (MetarRawInput input) => Execute(() => new MetarParser().Parse(input.Raw)));

app.MapGet("/api/metar/live/{icao}", async (string icao, IHttpClientFactory httpClientFactory) =>
{
    if (string.IsNullOrWhiteSpace(icao) || icao.Length < 4)
    {
        return Results.BadRequest(new { error = "ICAO musi mieć minimum 4 znaki." });
    }

    try
    {
        var client = httpClientFactory.CreateClient();
        var url = $"https://aviationweather.gov/api/data/metar?ids={icao.ToUpperInvariant()}&format=json";
        var response = await client.GetFromJsonAsync<List<AviationWeatherMetarDto>>(url);

        if (response is null || response.Count == 0 || string.IsNullOrWhiteSpace(response[0].RawOb))
        {
            return Results.NotFound(new { error = "Brak danych METAR dla podanego ICAO." });
        }

        var metar = response[0];
        var parsed = new MetarParser().Parse(metar.RawOb!);

        return Results.Ok(new
        {
            source = "NOAA AviationWeather.gov",
            station = metar.IcaoId,
            observed = metar.ObsTime,
            raw = metar.RawOb,
            parsed
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Błąd pobierania METAR: {ex.Message}");
    }
});

app.Run();

static IResult Execute<T>(Func<T> fn)
{
    try { return Results.Ok(fn()); }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (Exception ex) { return Results.Problem(ex.Message); }
}

static string RenderHomePage() => """
<!DOCTYPE html><html lang="pl"><head><meta charset="UTF-8"/><meta name="viewport" content="width=device-width, initial-scale=1.0"/>
<title>PPLA Planner Web</title>
<style>
:root{--bg:#0f172a;--panel:#1e293b;--txt:#e2e8f0;--muted:#93a4c2;--acc:#3b82f6;--ok:#22c55e;--warn:#f59e0b;--bad:#ef4444}
*{box-sizing:border-box} body{margin:0;font-family:Inter,Arial,sans-serif;background:var(--bg);color:var(--txt)} .wrap{max-width:1250px;margin:24px auto;padding:0 16px}
.hero{background:linear-gradient(120deg,#1d4ed8,#0f172a);padding:20px;border-radius:14px;margin-bottom:18px}.hero p{color:#dbeafe}
.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(290px,1fr));gap:16px}.card{background:var(--panel);border-radius:14px;padding:16px}
small{display:block;color:var(--muted);margin-bottom:10px}.section{margin-bottom:10px;font-weight:700}.field{margin-bottom:8px}
label{font-size:13px;color:#cbd5e1;display:block;margin-bottom:4px}input,textarea,button{width:100%;padding:10px;border-radius:8px;border:1px solid #334155;background:#0b1220;color:var(--txt)}
button{background:var(--acc);border:none;font-weight:600;cursor:pointer}.result{margin-top:10px;background:#020617;border-radius:8px;padding:10px;min-height:100px;white-space:pre-wrap}
.result.ok{border-left:4px solid var(--ok)}.result.err{border-left:4px solid var(--bad)}
</style></head><body><div class="wrap">
<div class="hero"><h1>PPLA Planner – Dashboard</h1><p>Każdy moduł ma opis, podpisane pola i walidację błędów API.</p></div>
<div class="grid">
<div class="card"><div class="section">Fuel Planner</div><small>Plan paliwa: trip, contingency, block fuel i czas lotu.</small>
<div class="field"><label>Dystans (NM)</label><input id="fd" value="120"></div><div class="field"><label>TAS (kt)</label><input id="ft" value="105"></div>
<div class="field"><label>Wiatr (+/- kt)</label><input id="fw" value="-10"></div><div class="field"><label>Spalanie (L/h)</label><input id="ff" value="24"></div>
<div class="field"><label>Taxi fuel (L)</label><input id="fx" value="3"></div><div class="field"><label>Reserve fuel (L)</label><input id="fr" value="12"></div>
<button onclick="fuel()">Przelicz Fuel</button><div id="fuelOut" class="result"></div></div>

<div class="card"><div class="section">Crosswind</div><small>Składowa czołowa/boczna i kontrola limitu crosswind.</small>
<div class="field"><label>Kierunek RWY (°)</label><input id="rh" value="270"></div><div class="field"><label>Kierunek wiatru (°)</label><input id="wd" value="230"></div>
<div class="field"><label>Prędkość wiatru (kt)</label><input id="ws" value="18"></div><div class="field"><label>Limit crosswind (kt)</label><input id="cl" value="15"></div>
<button onclick="wind()">Przelicz Crosswind</button><div id="windOut" class="result"></div></div>

<div class="card"><div class="section">VFR Leg</div><small>Heading, WCA, GS i czas odcinka VFR.</small>
<div class="field"><label>True course (°)</label><input id="vc" value="210"></div><div class="field"><label>Dystans (NM)</label><input id="vd" value="75"></div>
<div class="field"><label>TAS (kt)</label><input id="vt" value="102"></div><div class="field"><label>Wind direction (°)</label><input id="vwdir" value="250"></div>
<div class="field"><label>Wind speed (kt)</label><input id="vwsp" value="20"></div><div class="field"><label>Mag variation (°)</label><input id="vmag" value="4"></div>
<button onclick="vfr()">Przelicz VFR</button><div id="vfrOut" class="result"></div></div>

<div class="card"><div class="section">METAR</div><small>Tryb live (NOAA) + parser ręczny METAR/TAF.</small>
<div class="field"><label>ICAO (np. EPWA)</label><input id="icao" value="EPWA"></div><button onclick="liveMetar()">Pobierz live METAR</button>
<div id="metarOut" class="result"></div><hr style="border-color:#334155;margin:12px 0"><div class="field"><label>Ręczny METAR/TAF</label><textarea id="raw" rows="4">METAR EPWA 291200Z 23012KT 9999 SCT030 Q1013</textarea></div>
<button onclick="parseMetar()">Parsuj ręcznie</button><div id="rawOut" class="result"></div></div>
</div></div>
<script>
async function api(url,method,body){const r=await fetch(url,{method,headers:{'Content-Type':'application/json'},body:body?JSON.stringify(body):undefined});let data={};try{data=await r.json()}catch{};if(!r.ok){throw new Error(data.error||data.title||'Błąd API')}return data}
function render(id,data,isErr=false){const el=document.getElementById(id);el.textContent=JSON.stringify(data,null,2);el.className='result '+(isErr?'err':'ok')}
async function fuel(){try{render('fuelOut',await api('/api/fuel','POST',{distanceNm:+fd.value,trueAirspeedKt:+ft.value,windComponentKt:+fw.value,fuelBurnPerHourL:+ff.value,taxiFuelL:+fx.value,reserveFuelL:+fr.value,contingencyPercent:5}))}catch(e){render('fuelOut',{error:e.message},true)}}
async function wind(){try{render('windOut',await api('/api/wind','POST',{runwayHeadingDeg:+rh.value,windDirectionDeg:+wd.value,windSpeedKt:+ws.value,crosswindLimitKt:+cl.value}))}catch(e){render('windOut',{error:e.message},true)}}
async function vfr(){try{render('vfrOut',await api('/api/vfr','POST',{trueCourseDeg:+vc.value,distanceNm:+vd.value,trueAirspeedKt:+vt.value,windDirectionDeg:+vwdir.value,windSpeedKt:+vwsp.value,magneticVariationDeg:+vmag.value}))}catch(e){render('vfrOut',{error:e.message},true)}}
async function liveMetar(){try{render('metarOut',await api('/api/metar/live/'+icao.value,'GET'))}catch(e){render('metarOut',{error:e.message},true)}}
async function parseMetar(){try{render('rawOut',await api('/api/metar/parse','POST',{raw:raw.value}))}catch(e){render('rawOut',{error:e.message},true)}}
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
record AviationWeatherMetarDto(string? IcaoId, string? RawOb, string? ObsTime);
