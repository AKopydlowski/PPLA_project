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

## Dalszy rozwój
Szczegółowa propozycja roadmapy: `ROADMAP_ROZWOJU.md`.
