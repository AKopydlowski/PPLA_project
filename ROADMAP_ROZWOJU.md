# Propozycja dalszego rozwoju aplikacji PPLA

## 1) Priorytet: domknięcie MVP (funkcje z README)

Aktualny kod implementuje tylko moduł **Weight & Balance** (konsola). Aby aplikacja odpowiadała zakresowi z README, warto dodać pozostałe moduły jako osobne komponenty domenowe + osobne UI.

### Etap 1 (najbliższe sprinty)
1. **Fuel & Flight Planner**
   - wejścia: dystans, TAS, wiatr, spalanie, taxi fuel, reserve/contingency,
   - wyjścia: trip fuel, block fuel, EET/ETA,
   - walidacje i scenariusze awaryjne (np. brak rezerwy).
2. **Runway Wind / Crosswind**
   - obliczanie headwind/tailwind i crosswind dla RWY,
   - limity ostrzegawcze (samolot / pilot),
   - szybki tryb "what-if" dla kilku dróg startowych.
3. **METAR/TAF Parser**
   - parser tokenów + walidator,
   - mapowanie na model danych (wiatr, widzialność, chmury, QNH),
   - czytelny raport po polsku dla ucznia pilota.
4. **VFR Planner**
   - kurs magnetyczny/rzeczywisty, WCA, GS, EET,
   - odcinki trasy (legi) i podsumowanie całego lotu.

---

## 2) Refaktoryzacja architektury (żeby łatwo skalować)

### Proponowana struktura
- `Core/<Module>` – logika obliczeń i modele (bez `Console`),
- `Application` – use-case'y i orkiestracja modułów,
- `Infrastructure` – np. źródła pogody/API, zapis planów lotu,
- `UI/Console` (teraz) + przyszłościowo `UI/Web`.

### Kluczowe zmiany techniczne
- Wspólny **interfejs kalkulatora** (`ICalculationService`) i jednolity format wyników,
- `Result<T>`/`ValidationResult` zamiast wyjątków do walidacji danych wejściowych,
- `Units` (kg/lbs, kt, NM, m/ft) żeby uniknąć błędów jednostek,
- Separacja modelu domenowego od prezentacji (brak zależności Core -> Console).

---

## 3) Jakość i bezpieczeństwo obliczeń

1. **Testy jednostkowe i regresyjne**
   - golden cases dla znanych scenariuszy lotniczych,
   - testy brzegowe (zero fuel, silny wiatr boczny, maksymalny MTOW),
   - testy tolerancji numerycznej.
2. **Testy akceptacyjne**
   - scenariusz "od planowania do decyzji GO/NO-GO",
   - porównanie wyników z arkuszem referencyjnym.
3. **Ślad audytowy obliczeń**
   - opcjonalne logi kroków obliczeń (edukacyjnie + łatwe debugowanie).

---

## 4) UX dla pilota-ucznia (duża wartość edukacyjna)

- Tryb **„krok po kroku”** z wyjaśnieniem, skąd bierze się każdy wynik,
- **Checklisty przedlotowe** powiązane z kalkulatorami,
- Oznaczenie poziomu ryzyka (zielony / żółty / czerwony),
- Zapisywanie i wczytywanie „scenariuszy lotu” do nauki.

---

## 5) Plan wdrożenia na 90 dni

### Dni 1–30
- Ustalenie kontraktów modułów i walidacji,
- Implementacja Fuel Planner + Crosswind,
- Testy jednostkowe dla nowych modułów.

### Dni 31–60
- METAR/TAF parser i VFR planner,
- Wspólny model trasy + podsumowanie lotu,
- Raport wyników (tekst/JSON).

### Dni 61–90
- Refaktoryzacja UI na menu modułowe,
- Testy akceptacyjne end-to-end,
- Przygotowanie pod prosty interfejs webowy (np. ASP.NET minimal API + frontend).

---

## 6) Konkretny „next step” na teraz

Najbardziej opłacalny krok: **dodać Fuel & Flight Planner jako drugi moduł** i równolegle uruchomić projekt testów (`PPLA_project.Tests`). To da:
- szybki przyrost funkcjonalności,
- wzorzec dla kolejnych modułów,
- fundament jakości pod dalszy rozwój.
