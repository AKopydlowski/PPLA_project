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

## 4) UX/UI: zrobić aplikację „fajną” i nowoczesną

### Kierunek interfejsu
- Przejście z samej konsoli na **web UI (desktop-first, mobile-friendly)**,
- Dashboard z 4 kaflami modułów: Fuel, Wind, METAR/TAF, VFR,
- Szybkie przełączanie scenariuszy (np. "trening lokalny", "lot nawigacyjny").

### Smaczki designerskie (duża wartość)
- **Kolory statusów lotniczych**:
  - zielony = bezpiecznie,
  - bursztyn = uwaga,
  - czerwony = przekroczone limity.
- **Mikroanimacje i feedback**:
  - animacja przeliczania,
  - podświetlanie pól, które wpływają na alert,
  - tooltip "dlaczego taki wynik".
- **Karty wyniku premium**:
  - Crosswind jako półokrągły gauge,
  - Fuel jako pasek trip/reserve/block,
  - VFR legi jako timeline.
- **Tryb dzień/noc** (dark mode przydatny przed lotami porannymi i wieczornymi).

### Edukacyjne dodatki UX
- Tryb **"Pokaż wzór"** przy każdym wyniku,
- Tryb "Instruktor": komentarz, co poprawić w planie,
- Skróty klawiszowe dla szybkiej obsługi w aeroklubie.

---

## 5) Integracje z oficjalnymi źródłami danych (API)

### Co warto pobierać automatycznie
1. **METAR/TAF** z oficjalnych/instytucjonalnych źródeł lotniczych (zależnie od kraju operacji).
2. **NOTAM/AUP** (tam gdzie API i licencja na to pozwalają).
3. **Dane lotniskowe**:
   - pasy startowe (kierunki, długości),
   - elewacja lotniska,
   - częstotliwości (jeśli źródło dopuszcza publikację).

### Architektura integracji
- Warstwa `Infrastructure/Weather` i `Infrastructure/AviationData`,
- Adaptery per dostawca (np. `ImwProvider`, `NoaaProvider`, `AisProvider`),
- Cache + timestamp danych (np. "METAR z 12:20Z") żeby użytkownik widział świeżość danych.

### Zasady bezpieczeństwa i jakości danych
- Zawsze pokazywać: **źródło + czas aktualizacji**,
- Fallback na ręczne wpisanie danych, gdy API niedostępne,
- Walidacja i "confidence checks" (np. ostrzeżenie, gdy QNH odstaje od normy).

---

## 6) Plan wdrożenia na 90 dni

### Dni 1–30
- Ustalenie kontraktów modułów i walidacji,
- Implementacja Fuel Planner + Crosswind,
- Design system UI (kolory, komponenty, alerty),
- Start webowego MVP (np. Blazor/ASP.NET + prosty frontend).

### Dni 31–60
- METAR/TAF parser i VFR planner,
- Podpięcie pierwszego oficjalnego źródła METAR,
- Interaktywny dashboard i zapis scenariuszy,
- Testy jednostkowe + snapshot testy UI.

### Dni 61–90
- Kolejne integracje danych lotniczych (NOTAM/lotniska),
- Testy akceptacyjne end-to-end,
- Optymalizacja UX i accessibility,
- Beta release dla pilotów-uczniów/instruktorów.

---

## 7) Konkretny „next step” na teraz

Najbardziej opłacalny krok: **zrobić klikalny prototyp UI + podpiąć 1 oficjalne API METAR**. To da:
- natychmiastowy efekt „wow” wizualnie,
- realną wartość operacyjną (mniej ręcznego przepisywania),
- fundament pod kolejne moduły i dalsze integracje.
