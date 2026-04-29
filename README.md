# PPLA_project

Wstępny projekt aplikacji edukacyjnej dla pilotów PPLA, przygotowany w C# z wykorzystaniem Visual Studio.

## Opis
Aplikacja konsolowa zawiera menu modułów i podstawowe narzędzia lotnicze:

1. **Weight & Balance (W&B)** – oblicza masę startową i środek ciężkości.
2. **Fuel & Flight Planner** – planowanie czasu lotu, trip fuel, contingency i block fuel.
3. **Runway Wind / Crosswind Calculator** – oblicza headwind/tailwind oraz crosswind z kontrolą limitu.
4. **METAR/TAF Parser** – prosty parser ręcznie wprowadzonego METAR/TAF.
5. **VFR Planner** – oblicza heading, ground speed, czas lotu i wind correction angle dla odcinka VFR.

## Uruchomienie
1. Zbuduj projekt: `dotnet build`
2. Uruchom aplikację: `dotnet run`
3. Wybierz moduł z menu głównego.

## Web UI + API
Domyślnie aplikacja uruchamia dashboard webowy (`dotnet run`) i udostępnia endpointy API dla modułów obliczeniowych oraz live METAR z AviationWeather (NOAA).

- Strona główna: `http://localhost:5000` (lub port z logów).
- Konsola legacy: `dotnet run -- --console`.
- Live METAR API: `GET /api/metar/live/{ICAO}`.

## Nowe moduły (2026)
- `POST /api/scenario/go-no-go` — decyzja GO/NO-GO z naruszeniami i rekomendacjami.
- `POST /api/metar/v2/parse` — parser METAR/TAF v2 (tokenizer + walidator + plain language PL).
- `POST /api/history/flight-plan` i `GET /api/history/flight-plan` — zapis i historia planów lotu (audit trail).
- Profile: samolotu i pilota używane w scenariuszach.


## Ważne zastrzeżenie bezpieczeństwa
Aplikacja ma **wyłącznie charakter edukacyjny i treningowy**. Wyniki kalkulatorów (paliwo, wiatr, VFR, W&B, METAR/TAF) mogą zawierać uproszczenia oraz błędy statystyczne/numeryczne, dlatego **nie mogą być jedyną podstawą decyzji operacyjnych**. Nie używaj tej aplikacji jako jedynego źródła do planowania i wykonywania rzeczywistych lotów. Zawsze weryfikuj dane i decyzje zgodnie z oficjalnymi procedurami lotniczymi, aktualnymi publikacjami i z instruktorem/operatorem.


### Uruchamianie w Visual Studio
- Wybierz profil startowy **PPLA_project** (web) aby VS automatycznie otworzył przeglądarkę.
- Jeśli przeglądarka się nie otworzy, wejdź ręcznie na: `http://localhost:5000`.
- Profil **PPLA_project (console)** uruchamia tryb tekstowy (`--console`) bez strony WWW.
