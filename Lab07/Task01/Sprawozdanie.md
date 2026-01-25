# Kryptografia i kryptoanaliza

## Laboratorium 7

### Grupa 1ID24B

### Autorzy: Kamil Fudala, Andrzej Szczytyński

## Wprowadzenie
Rozwój algorytmów strumieniowych ewoluował od prostych, liniowych konstrukcji do złożonych układów nieliniowych. Klasyczne rejestry przesuwne ze sprzężeniem liniowym (LFSR) cechują się wysoką wydajnością sprzętową i dobrymi właściwościami statystycznymi, jednak ich liniowość czyni je podatnymi na ataki algebraiczne, takie jak algorytm Berlekampa-Massey'a, który pozwala na odtworzenie wielomianu sprzężenia zwrotnego na podstawie $2L$ bitów strumienia (gdzie $L$ to długość rejestru). Generatory złożone, łączące kilka LFSR, również wykazały podatności, głównie na ataki korelacyjne, które wykorzystują statystyczne zależności między wyjściem generatora a stanami poszczególnych rejestrów.

Trivium, jako reprezentant nowoczesnych szyfrów strumieniowych (NLFSR), został zaprojektowany z myślą o wyeliminowaniu tych słabości. Jest to szyfr sprzętowy wybrany do portfolio projektu eSTREAM (Profil 2), oparty na trzech nieliniowych rejestrach przesuwnych ze sprzężeniem zwrotnym. Jego konstrukcja zapewnia prostotę implementacji sprzętowej przy jednoczesnym zachowaniu wysokiego poziomu bezpieczeństwa i wydajności.

Parametry systemowe Trivium obejmują:
*   Klucz ($K$): 80 bitów.
*   Wektor inicjalizujący ($IV$): 80 bitów.
*   Stan wewnętrzny: 288 bitów.

Celem niniejszego laboratorium jest weryfikacja poprawności implementacji szyfru Trivium, analiza bezpieczeństwa fazy rozgrzewania (warm-up), zbadanie ewolucji stanu wewnętrznego oraz praktyczna demonstracja ataków typu "IV Reuse" (ponowne użycie wektora inicjalizującego) oraz ataku kostkowego (Cube Attack) na wersjach o zredukowanej liczbie rund.

## Opis implementacji

### Architektura rozwiązania
Stan wewnętrzny szyfru Trivium o długości 288 bitów jest podzielony na trzy rejestry przesuwne o różnych długościach:
*   **Rejestr A**: 93 bity ($s_1, \dots, s_{93}$).
*   **Rejestr B**: 84 bity ($s_{94}, \dots, s_{177}$).
*   **Rejestr C**: 111 bitów ($s_{178}, \dots, s_{288}$).

Proces inicjalizacji polega na załadowaniu 80-bitowego klucza $K$ do pierwszych pozycji rejestru A oraz 80-bitowego wektora $IV$ do pierwszych pozycji rejestru B. Pozostałe bity w tych rejestrach są zerowane. Rejestr C jest inicjalizowany zerami, z wyjątkiem trzech ostatnich bitów ($s_{286}, s_{287}, s_{288}$), które są ustawiane na wartość 1. Stanowi to warunek konieczny do uniknięcia stanu zerowego. Następnie algorytm wykonuje 1152 rundy "rozgrzewania" (4 pełne cykle po 288 kroków), podczas których stan jest aktualizowany, ale nie jest generowany żaden strumień wyjściowy. Ma to na celu pełne wymieszanie bitów klucza i IV.

### Diagram przepływu danych
```mermaid
graph TD
    subgraph "Internal State (288 bits)"
        subgraph "Register A (93 bits)"
            A_reg[State s1...s93]
        end
        subgraph "Register B (84 bits)"
            B_reg[State s94...s177]
        end
        subgraph "Register C (111 bits)"
            C_reg[State s178...s288]
        end
    end

    Input_Key[Key (80 bits)] --> A_reg
    Input_IV[IV (80 bits)] --> B_reg
    Const[Constants 111] --> C_reg

    %% Taps
    A_reg -- s66, s93 --> XOR_A[XOR]
    B_reg -- s162, s177 --> XOR_B[XOR]
    C_reg -- s243, s288 --> XOR_C[XOR]

    %% Output Generation
    XOR_A --> XOR_Out[XOR Sum]
    XOR_B --> XOR_Out
    XOR_C --> XOR_Out
    XOR_Out --> Keystream[Keystream Bit z]

    %% Feedback Logic (Non-linear)
    A_reg -- s91 & s92 --> AND_A[AND]
    B_reg -- s175 & s176 --> AND_B[AND]
    C_reg -- s286 & s287 --> AND_C[AND]

    XOR_A --> Sum_A[Feedback A]
    AND_A --> Sum_A
    C_reg -- s264 (Cross) --> Sum_A

    XOR_B --> Sum_B[Feedback B]
    AND_B --> Sum_B
    A_reg -- s69 (Cross) --> Sum_B

    XOR_C --> Sum_C[Feedback C]
    AND_C --> Sum_C
    B_reg -- s171 (Cross) --> Sum_C

    %% State Update
    Sum_C --> A_reg
    Sum_A --> B_reg
    Sum_B --> C_reg
```

### Kluczowe algorytmy

#### `TriviumCipher.cs` – Rdzeń kryptograficzny i optymalizacja niskopoziomowa

Implementacja klasy `TriviumCipher` stanowi przykład inżynierii nastawionej na ekstremalną wydajność (High-Performance Computing) w środowisku .NET, osiągając przepustowość rzędu 70 Gbps na architekturze Zen 3. Osiągnięcie takiego wyniku wymagało zastosowania szeregu zaawansowanych technik optymalizacyjnych:

1.  **Vectored Instruction Sets (AVX2 Intrinsics)**:
    Kluczowym elementem optymalizacji jest wykorzystanie instrukcji wektorowych SIMD (Single Instruction, Multiple Data) poprzez typ `Vector256<ulong>`. W metodach `UpdateStateV256_Inline` oraz `CalculateZ_And_Update`, stan wewnętrzny jest przetwarzany równolegle. Ponieważ Trivium pozwala na wygenerowanie do 64 bitów bez zależności zwrotnej (dependency chain), możliwe jest obliczanie wielu kroków algorytmu w jednym cyklu zegara procesora, wykorzystując pełną szerokość 256-bitowych rejestrów YMM.

2.  **Unsafe Context & Pointer Arithmetic**:
    Kod wykorzystuje słowo kluczowe `unsafe` oraz wskaźniki (`byte*`, `ulong*`), co pozwala na bezpośrednią manipulację pamięcią. Eliminuje to narzut związany ze sprawdzaniem granic tablic (Array Bounds Checks), który jest standardem w bezpiecznym kodzie C#. W krytycznych pętlach (`EncryptSequential`) bezpośredni dostęp do buforów pamięci przekłada się na generowanie bardziej zwartego kodu maszynowego, pozbawionego zbędnych instrukcji skoków warunkowych.

3.  **Memory Layout Optimization & HugePages**:
    Zastosowanie `GC.AllocateUninitializedArray` pozwala na alokację dużych bloków pamięci bez ich wstępnego zerowania (Zero-Initialization), co jest kluczowe przy buforach rzędu 1 GB. Dodatkowo, system operacyjny skonfigurowano do użycia HugePages (strony pamięci o rozmiarze 2 MB zamiast standardowych 4 KB). Zmniejsza to presję na bufor TLB (Translation Lookaside Buffer), redukując liczbę "TLB Misses" podczas dostępu do dużych, ciągłych obszarów pamięci.

4.  **Instruction Level Parallelism (ILP) & Prefetching**:
    Struktura pętli została zaprojektowana tak, aby maksymalizować ILP. Rozwijanie pętli (loop unrolling) w `ProcessByteBatch8` oraz ręczne wskazówki dla procesora (`Sse.Prefetch0`) pozwalają na efektywne wykorzystanie potoku wykonawczego (pipeline) i hierarchii pamięci cache, minimalizując czasy oczekiwania na dane z pamięci RAM.

```csharp
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Task01.Domain.Core;

[SkipLocalsInit]
public unsafe class TriviumCipher : ITriviumCipher
{
    // ... (Constants and Fields)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateStateV256_Inline(ref Vector256<ulong> a0, ref Vector256<ulong> a1, ref Vector256<ulong> b0,
        ref Vector256<ulong> b1, ref Vector256<ulong> c0, ref Vector256<ulong> c1)
    {
        var t1 = Avx2.Xor(Avx2.Or(Avx2.ShiftRightLogical(a0, 27), Avx2.ShiftLeftLogical(a1, 37)), a0);
        var t2 = Avx2.Xor(Avx2.Or(Avx2.ShiftRightLogical(b0, 15), Avx2.ShiftLeftLogical(b1, 49)), b0);
        var t3 = Avx2.Xor(Avx2.Or(Avx2.ShiftRightLogical(c0, 45), Avx2.ShiftLeftLogical(c1, 19)), c0);
        // ... (Full AVX2 Logic)
    }

    // ... (Full implementation as seen in source)
}
```

#### `CubeAttackService.cs` – Kryptoanaliza algorytmiczna

Serwis ten implementuje atak kostkowy (Cube Attack), potężną technikę kryptoanalityczną przeciwko szyfrom strumieniowym. Atak podzielony jest na dwie fazy:

*   **Faza Offline (Pre-computation)**: Algorytm losuje podzbiory indeksów IV (kostki) i testuje je pod kątem liniowości superwielomianu. Jeśli suma wyjść szyfru dla wszystkich możliwych wartości kostki zależy liniowo od bitów klucza, kostka ta jest uznawana za użyteczną.
*   **Faza Online**: Dla znalezionych kostek liniowych, system rozwiązuje układ równań liniowych w ciele $GF(2)$, odzyskując fragmenty tajnego klucza. Wykorzystano tutaj równoległość (`Parallel.For`) do przyspieszenia obliczeń sum kostkowych.

```csharp
using System.Diagnostics;
using Task01.Domain.Core;
using Task01.Domain.Math;

namespace Task01.Domain.Services;

public class CubeAttackService(ITriviumCipher cipher)
{
    // ...
    public List<(Cube Cube, int KeyIndex)> FindLinearCubes(int rounds)
    {
        // Heuristic search for linear cubes (Offline Phase)
        // ...
    }

    public static bool[] RecoverKey(List<(Cube Cube, int KeyIndex)> linearCubes, ITriviumCipher oracle, int rounds)
    {
        var swOnline = Stopwatch.StartNew();
        var results = new bool[linearCubes.Count];

        Parallel.For(0, linearCubes.Count, i =>
        {
            // Compute cube sums in parallel (Online Phase)
            // ...
        });

        // Construct matrix and solve system
        var matrix = new List<bool[]>();
        foreach (var item in linearCubes)
        {
            var row = new bool[80];
            row[item.KeyIndex] = true;
            matrix.Add(row);
        }

        var recoveredBits = Gf2Solver.SolveLinearSystem(matrix, results, 80);
        return recoveredBits;
    }
}
```

#### `Gf2Solver.cs` – Rozwiązywanie układów równań w ciele Galois

Jest to wyspecjalizowana implementacja eliminacji Gaussa dla macierzy nad ciałem $GF(2)$. Zamiast operować na liczbach zmiennoprzecinkowych, solver działa na bitach upakowanych w zmiennych `ulong`.

*   **Bit-packing**: Wiersze macierzy są reprezentowane jako struktury `Row` zawierające dwa pola `ulong` (Low/High), co pozwala na reprezentację do 128 zmiennych (wystarczające dla 80-bitowego klucza).
*   **XOR Operations**: Dodawanie wierszy w ciele $GF(2)$ sprowadza się do operacji XOR (`^=`), która jest wykonywana równolegle na 64 bitach (całym słowie maszynowym). Zapewnia to ogromną wydajność w porównaniu do klasycznych implementacji operujących na pojedynczych elementach tablicy.

```csharp
using System.Numerics;

namespace Task01.Domain.Math;

public static class Gf2Solver
{
    public static bool[] SolveLinearSystem(List<bool[]> matrix, bool[] results, int variableCount)
    {
        // Gaussian elimination over GF(2) with bit-packed rows
        // ...
            for (var r = 0; r < rowCount; r++)
            {
                if (r != pivotRow)
                {
                    // Row addition corresponds to XOR
                    if (isSet)
                    {
                        rows[r].Low ^= rows[pivotRow].Low;
                        rows[r].High ^= rows[pivotRow].High;
                    }
                }
            }
        // ...
    }
}
```

## Wyniki eksperymentów
Poniższe wyniki uzyskano uruchamiając rozwiązanie na procesorze **Ryzen 7 5800H (Zen 3)** w środowisku Linux, wykorzystując kompilację AOT oraz obsługę HugePages.

### Eksperyment 1: Weryfikacja (Test Vectors)
Pierwszy eksperyment potwierdził zgodność generowanego strumienia z wektorami testowymi oraz poprawność operacji inwolutywnej (szyfrowanie i deszyfrowanie).

Tabela 1: Wyniki weryfikacji poprawności

| Parametr | Wynik |
| :--- | :--- |
| Generated Hash | `FBE0BF265859051B517A2E4E239FC97F563203161907CF2DE7A8790FA1B2E9CD` |
| Expected Hash | `FBE0BF265859051B517A2E4E239FC97F563203161907CF2DE7A8790FA1B2E9CD` |
| Match | **True** |
| Involutive Check | **True** |

### Eksperyment 2: Atak IV Reuse
Zdemonstrowano atak polegający na ponownym użyciu wektora IV (IV Reuse). Przechwycono dwa szyfrogramy zaszyfrowane tym samym kluczem i IV. Wykorzystując znany fragment tekstu jawnego ("Crib"), odzyskano treść drugiej wiadomości.

*   Czas szyfrowania: 9.80 µs
*   Odzyskana wiadomość: `HTTP/1.1 404 Not Found`
*   Analiza dopasowań (Crib Dragging):
    *   'HTTP': 42 dopasowania (0.90 µs)
    *   'Content-Type:': 13 dopasowań (2.80 µs)

### Eksperyment 3: Analiza Rund i Ewolucja Stanu
Przeprowadzono analizę właściwości statystycznych strumienia w zależności od liczby rund rozgrzewania.

Tabela 2: Statystyki strumienia w funkcji liczby rund

| Rounds | Ones | Balance | Chi-Sq | Warmup (µs) | Throughput (Mbps) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 0 | 83 | 0.288 | 2.25 | 0.00 | 6659.52 |
| 192 | 154 | 0.535 | 3.10 | 0.00 | 6789.05 |
| 288 | 156 | 0.542 | 3.31 | 0.00 | 8447.23 |
| 384 | 142 | 0.493 | 3.61 | 0.00 | 8452.23 |
| 480 | 149 | 0.517 | 3.69 | 0.00 | 8339.03 |
| 576 | 149 | 0.517 | 3.31 | 0.00 | 7437.93 |
| 768 | 143 | 0.497 | 4.00 | 0.00 | 7346.35 |
| 1152 | 154 | 0.535 | 3.69 | 0.00 | 6534.92 |

Zauważalna jest stabilizacja balansu (blisko 0.5) oraz wartości testu Chi-kwadrat wraz ze wzrostem liczby rund.

### Eksperyment 4: Atak Kostkowy (Cube Attack)
Przeprowadzono atak kostkowy na wersjach Trivium o zredukowanej liczbie rund (192, 288, 384, 480).

Tabela 3: Wyniki ataku kostkowego (Podsumowanie)

| Rundy | Znalezione bity | Dokładność (%) | Czas Offline (µs) | Czas Online (µs) |
| :--- | :--- | :--- | :--- | :--- |
| 192 | 26 | 46.2 % | 7899.80 | 260.30 |
| 288 | 26 | 38.5 % | 6258.20 | 166.50 |
| 384 | 23 | 52.2 % | 6538.80 | 85.90 |
| 480 | 16 | 56.2 % | 6626.60 | 73.00 |

### Eksperyment 5: Porównanie Statystyczne
Porównano jakość generatora dla 0, 288 i 1152 rund (standard).

Tabela 4: Testy statystyczne (długość próby: 1000000 bitów)

| Rundy | Częstość "1" (%) | Liczba serii (Runs) | Autokorelacja (Lag 1) | Chi-Square | Wniosek |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 0 | 50.03 % | 499359 | 0.0013 | 0.4651 | Pass |
| 288 | 50.03 % | 499369 | 0.0013 | 0.4624 | Pass |
| 1152 | 50.03 % | 499382 | 0.0012 | 0.4462 | Pass |

Wszystkie warianty przeszły podstawowe testy statystyczne (wartość krytyczna Chi-Sq: 3.841), co sugeruje, że słabości wczesnych rund mają charakter algebraiczny, a nie czysto statystyczny.

### Eksperyment 6: Test Nasycenia (Saturation)
Test wydajnościowy na próbce 1 miliarda bitów (1 Gbit).
*   **Czas generacji**: 638.84 ms
*   **Prędkość**: 1565.34 Mbps (KeyStream)
*   **Szybkość szyfrowania (Native)**: **70.05 Gbps**
*   **Integrity Check**: True

## Pytania Kontrolne

1.  **Różnica LFSR vs NLFSR**:
    LFSR (Linear Feedback Shift Register) jest układem liniowym, co oznacza, że każdy bit wyjściowy jest liniową kombinacją bitów stanu. Sprawia to, że jego złożoność liniowa jest niska, a stan wewnętrzny można odtworzyć algorytmem Berlekampa-Massey'a. NLFSR (Non-Linear Feedback Shift Register), taki jak w Trivium, wprowadza operacje nieliniowe (np. mnożenie bitów - bramki AND) w funkcji sprzężenia zwrotnego. Zwiększa to drastycznie złożoność algebraiczną i utrudnia ataki polegające na rozwiązywaniu układów równań liniowych.

2.  **Dowód formalny $C_1 \oplus C_2 = P_1 \oplus P_2$**:
    Niech $C_1$ i $C_2$ będą szyfrogramami powstałymi z tekstów jawnych $P_1$ i $P_2$ przy użyciu tego samego strumienia klucza $Z$ (ten sam Klucz i IV).
    $C_1 = P_1 \oplus Z$
    $C_2 = P_2 \oplus Z$
    Obliczając różnicę symetryczną (XOR) szyfrogramów:
    $C_1 \oplus C_2 = (P_1 \oplus Z) \oplus (P_2 \oplus Z) = P_1 \oplus P_2 \oplus (Z \oplus Z) = P_1 \oplus P_2 \oplus 0 = P_1 \oplus P_2$.
    Zależność ta pozwala na atak, jeśli znamy jeden tekst jawny lub jego statystykę, co ujawnia treść drugiego tekstu bez znajomości klucza.

3.  **Znaczenie fazy rozgrzewania**:
    Faza rozgrzewania (w Trivium 1152 rundy) służy do dyfuzji bitów klucza i IV na cały stan wewnętrzny (288 bitów). Bez tej fazy, pierwsze bity strumienia byłyby silnie skorelowane z kluczem i IV, a nieliniowość układu nie zdążyłaby wpłynąć na wyjście. Pozwoliłoby to na trywialne ataki algebraiczne lub odgadnięcie stanu.

4.  **Sprzężenia krzyżowe**:
    W Trivium wyjście rejestru A zasila wejście B, wyjście B zasila C, a wyjście C zasila A. Ta cykliczna struktura powoduje, że okres generatora jest znacznie dłuższy niż suma okresów poszczególnych rejestrów, a bity stanu są wielokrotnie mieszane między rejestrami, co wzmacnia odporność na ataki typu "dziel i rządź".

5.  **Definicja kostki i superwielomianu**:
    W ataku kostkowym (Cube Attack), "kostka" to zbiór bitów IV, dla których sumujemy wartości wyjściowe szyfru. Superwielomian to wielomian opisujący zależność sumy wyjść (dla wszystkich wartości kostki) od tajnych bitów klucza. Jeśli superwielomian jest liniowy, możemy łatwo wyznaczyć bity klucza.

6.  **Porównanie IV Reuse vs Atak korelacyjny**:
    Atak IV Reuse (Two-Time Pad) jest błędem implementacyjnym/protokolarnym – wynika z niewłaściwego użycia szyfru i jest zazwyczaj deterministyczny i natychmiastowy. Atak korelacyjny jest atakiem kryptoanalitycznym na samą konstrukcję szyfru (słabość algorytmu), wymagającym dużej ilości danych i analizy statystycznej w celu znalezienia korelacji między wyjściem a stanem wewnętrznym.

7.  **Margines bezpieczeństwa**:
    Trivium z 1152 rundami rozgrzewania jest uważane za bezpieczne. Ataki (np. kostkowe) są skuteczne dla wersji zredukowanych do około 700-800 rund. Oznacza to, że margines bezpieczeństwa jest stosunkowo niewielki (thin margin), ale wystarczający przy obecnym stanie wiedzy.

8.  **Modyfikacje protokołu**:
    W celu zwiększenia bezpieczeństwa można zwiększyć liczbę rund rozgrzewania lub długość rejestrów (jak w Trivium-A). Jednak każda zmiana musi być ostrożna, aby nie wpłynąć negatywnie na wydajność sprzętową, która jest głównym atutem Trivium.

## Podsumowanie i wnioski końcowe

*   Trivium jest niezwykle wydajnym algorytmem, szczególnie w implementacjach wykorzystujących SIMD i AOT.
*   Optymalizacja niskopoziomowa (AVX2, HugePages, `unsafe` code) pozwala na osiągnięcie prędkości rzędu 70 Gbps, jednak wiąże się to ze znacznym obniżeniem czytelności i łatwości utrzymania kodu (trade-off: wydajność vs maintainability).
*   Faza rozgrzewania (1152 rundy) jest krytyczna dla odporności na ataki algebraiczne (Cube), mimo że właściwości statystyczne strumienia są poprawne znacznie wcześniej.
*   Ponowne użycie IV jest błędem krytycznym, pozwalającym na całkowitą eliminację strumienia klucza bez znajomości klucza $K$.
