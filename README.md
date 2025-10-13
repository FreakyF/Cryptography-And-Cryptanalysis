# Kryptografia i kryptoanaliza

## Laboratorium 1

### Grupa 1ID24B

### Autorzy: Kamil Fudala, Andrzej Szczytyński

### Zadanie 1

Dokonaj implementacji programu szyfrującego i deszyfrującego zadany tekst.

1. Tekst jawny powinien być importowany do programu z pliku tekstowego, którego nazwa określona powinna być
   po zdefiniowanym argumencie / fladze: -i.
2. Wynik pracy programu powinien być eksportowany do pliku tekstowego, którego nazwa określona powinna być
   po zdefiniowanym argumencie / fladze: -o.
3. Klucz powinien być importowany z pliku tekstowego, którego nazwa powinna być określona po zdefiniowanym
   argumencie / fladze: -k.
4. Tryb pracy programu powinien być określony poprzez flagi: -e dla procesu szyfrowania, -d dla procesu deszyfrowania.

#### Implementacja

#### Wyniki

Przeprowadzono szyfrowanie tekstu jawnego kluczem podstawieniowym, co dało szyfrogram istotnie różny od oryginału.
Wykonano następnie deszyfrowanie, które odtworzyło treść w postaci znormalizowanej do wielkich liter A-Z bez znaków
nieliterowych. Wyniki są zgodne z założeniami, algorytm działa poprawnie, a odzyskany tekst pokrywa się z
przetworzonym wejściem.

* Szyfrowanie
    ```
    ➜  Task01 (main) dotnet run --project Task01/Task01.csproj -- -e -k key.txt -i plaintext.txt -o ciphertext.txt                                                                                                                           ✗ ✭
    ➜  Task01 (main) diff plaintext.txt ciphertext.txt                                                                                                                                                                                       ✗ ✭
    1,4c1
    < Once upon a time, in a quiet village surrounded by hills and forests, there lived a young boy who loved to read stories. 
    < He spent his evenings by the fireplace, turning the pages of old books filled with adventures and mysteries. 
    < The more he read, the more he dreamed of distant lands and brave heroes. 
    < One day, he decided that his own life should become a story worth telling.
    ---
    > GFETXHGFQZODTOFQJXOTZCOSSQUTLXKKGXFRTRWNIOSSLQFRYGKTLZLZITKTSOCTRQNGXFUWGNVIGSGCTRZGKTQRLZGKOTLITLHTFZIOLTCTFOFULWNZITYOKTHSQETZXKFOFUZITHQUTLGYGSRWGGALYOSSTRVOZIQRCTFZXKTLQFRDNLZTKOTLZITDGKTITKTQRZITDGKTITRKTQDTRGYROLZQFZSQFRLQFRWKQCTITKGTLGFTRQNITRTEORTRZIQZIOLGVFSOYTLIGXSRWTEGDTQLZGKNVGKZIZTSSOFU
    \ No newline at end of file
    ```

* Deszyfrowanie
    ```
    ➜  Task01 (main) dotnet run --project Task01/Task01.csproj -- -d -k key.txt -i ciphertext.txt -o recovered.txt                                                                                                                           ✗ ✭
    ➜  Task01 (main) diff plaintext.txt recovered.txt                                                                                                                                                                                          ✭
    1,4c1
    < Once upon a time, in a quiet village surrounded by hills and forests, there lived a young boy who loved to read stories. 
    < He spent his evenings by the fireplace, turning the pages of old books filled with adventures and mysteries. 
    < The more he read, the more he dreamed of distant lands and brave heroes. 
    < One day, he decided that his own life should become a story worth telling.
    ---
    > ONCEUPONATIMEINAQUIETVILLAGESURROUNDEDBYHILLSANDFORESTSTHERELIVEDAYOUNGBOYWHOLOVEDTOREADSTORIESHESPENTHISEVENINGSBYTHEFIREPLACETURNINGTHEPAGESOFOLDBOOKSFILLEDWITHADVENTURESANDMYSTERIESTHEMOREHEREADTHEMOREHEDREAMEDOFDISTANTLANDSANDBRAVEHEROESONEDAYHEDECIDEDTHATHISOWNLIFESHOULDBECOMEASTORYWORTHTELLING
    \ No newline at end of file
    ```

### Zadanie 2

Rozbudować program z poprzedniego przykładu poprzez dodanie do niego funkcjonalności generowania statystyk liczności
występowania n-gramów (sekwencji kolejnych liter), to jest mono-gramów (pojedynczych liter), bi-gramów (wyrazów
dwuliterowych), tri-gramów (wyrazów trzyliterowych) oraz quad-gramów (wyrazów czteroliterowych). Funkcjonalność ta
powinna być wyzwalana poprzez dodanie do programu jednej z następujących flag: -g1, -g2, -g3 lub
-g4, po której powinna zostać określona nazwa pliku, do którego zapisane zostaną wyniki.

#### Implementacja

#### Wyniki

Wygenerowano statystyki częstości mono-, bi-, tri- i quad-gramów z uprzednio znormalizowanego tekstu (A-Z).
Zliczenia zapisano do plików monograms.txt, bigrams.txt, trigrams.txt, quadgrams.txt w formacie "n-gram liczność".
Wyniki są zgodne z założeniami: dla wyższych n dominuje liczność 1, a otrzymane listy poprawnie odzwierciedlają n-gramy
występujące w tekście.

* Monogramy

    ```
    ➜  Task02 (main) dotnet run --project Task02/Task02.csproj -- -i plaintext.txt -g1 monograms.txt                   ✭
    ➜  Task02 (main) cat monograms.txt | tail -n 10                                                                    ✭
    B 6
    F 6
    G 6
    M 6
    V 6
    C 4
    P 4
    W 4
    K 1
    Q 1  
    ```

* Bigramy

    ```
    ➜  Task02 (main) dotnet run --project Task02/Task02.csproj -- -i plaintext.txt -g2 bigrams.txt                     ✭
    ➜  Task02 (main) cat bigrams.txt | tail -n 10                                                                      ✭
    UL 1
    UP 1
    VI 1
    WH 1
    WI 1
    WN 1
    WO 1
    YO 1
    YS 1
    YT 1 
    ```

* Trigramy

    ```
    ➜  Task02 (main) dotnet run --project Task02/Task02.csproj -- -i plaintext.txt -g3 trigrams.txt                    ✭
    ➜  Task02 (main) cat trigrams.txt | tail -n 10                                                                     ✭
    WIT 1
    WNL 1
    WOR 1
    YHE 1
    YHI 1
    YOU 1
    YST 1
    YTH 1
    YWH 1
    YWO 1                                                                                               ✭
    ```

* Quadgramy

    ```
    ➜  Task02 (main) dotnet run --project Task02/Task02.csproj -- -i plaintext.txt -g4 quadgrams.txt                   ✭
    ➜  Task02 (main) cat quadgrams.txt | tail -n 10                                                                    ✭
    WITH 1
    WNLI 1
    WORT 1
    YHED 1
    YHIL 1
    YOUN 1
    YSTE 1
    YTHE 1
    YWHO 1
    YWOR 1 
    ```

### Zadanie 3

Uzupełnij program z poprzedniego zadania, tak aby w przypadku podania flagi -rX, gdzie X jest liczbą należącą do
zbioru {1, 2, 3, 4} a następnie nazwy pliku, program odczytywał z niego referencyjną bazę n-gramów. Liczby z
podanego zbioru odpowiadają: {mono-gramom, bi-gramom, tri-gramom, quad-gramom}.

#### Implementacja

#### Wyniki

Zbudowano bazy referencyjne n-gramów (1-4) z pliku corpus.txt, który zawiera kilka książek pozyskanych z GITenberg.
Obliczono statystykę $\chi^2$ dla znormalizowanego plaintext.txt względem tych baz, uzyskując kolejno: 23.92 (mono),
415.66 (
bi), 5231.10 (tri), 45365.80 (quad).
Zaobserwowany wzrost wartości wraz z rzędem n-gramów jest zgodny z założeniami (więcej rzadkich klas i mniejsze $E_i$).

* Bazy referencyjne

    ```
    ➜  Task03 (main) dotnet run --project Task03/Task03.csproj -- -i corpus.txt -b1 mono_ref.txt -b2 bi_ref.txt -b3 tri_ref.txt -b4 quad_ref.txt                                                                                               ✭
    ```

* Monogramy

    ```
    ➜  Task03 (main) dotnet run --project Task03/Task03.csproj -- -i plaintext.txt -s -r1 mono_ref.txt                                                                                                                                         ✭
    23.920623209756698
    ```

* Bigramy

    ```
    ➜  Task03 (main) dotnet run --project Task03/Task03.csproj -- -i plaintext.txt -s -r2 bi_ref.txt                                                                                                                                           ✭
    415.6565528488983
    ```

* Trigramy

    ```
    ➜  Task03 (main) dotnet run --project Task03/Task03.csproj -- -i plaintext.txt -s -r3 tri_ref.txt                                                                                                                                          ✭
    5231.103323340586
    ```

* Quadgramy

    ```
    ➜  Task03 (main) dotnet run --project Task03/Task03.csproj -- -i plaintext.txt -s -r4 quad_ref.txt                                                                                                                                         ✭
    45365.80049320461 
    ```

### Zadanie 4

Wykonać eksperymenty::

* Dokonaj obserwacji wyniku testu $\chi^2$ dla tekstu jawnego i zaszyfrowanego o różnych długościach.
* Wiadomo, iż wynik testu może być znacząco zaburzony w przypadku gdy brane są pod uwagę symbole (n-gramy),
  które rzadko występują w tekście, np w przypadku mono-gramów języka angielskiego są to litery: J, K, Q, X oraz
  Z (patrz odczytana tablica częstości mono-gramów). Zbadaj wynik testu $\chi^2$ w przypadku gdy do wyznaczenia
  testu pominięte zostaną rzadko występujące n-gramy.

#### Implementacja

#### Wyniki

Zbudowano bazy referencyjne n-gramów (1-4) z pliku corpus.txt zawierającego kilka książek z GITenberg i wykonano
eksperymenty $\chi^2$ dla znormalizowanego tekstu jawnego oraz szyfrogramu przy różnych długościach próbek.
Zaobserwowano stabilizację i niskie wartości $\chi^2$ dla tekstu jawnego oraz wielokrotnie wyższe wartości dla
szyfrogramu.
Wykluczenie rzadkich n-gramów i próg $E_i$ obniżyły wynik zgodnie z oczekiwanym wpływem rzadkich klas.
Część wysokich wartości oraz komunikaty o brakujących n-gramach wynika z ograniczonego corpus.txt (zwłaszcza dla
4-gramów). Powiększenie korpusu, zastosowanie progu $E_i$ lub smoothingu redukuje ten efekt, co pozostaje zgodne z
założeniami.

* Bazy referencyjne

    ```
    ➜  Task04 (main) dotnet run --project Task04/Task04.csproj -- -i corpus.txt -b1 mono_ref.txt -b2 bi_ref.txt -b3 tri_ref.txt -b4 quad_ref.txt                                                                                               ✭
    ```

* Tekst jawny dla różnych długości próbek

    ```
    ➜  Task04 (main) dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r1 mono_ref.txt -nlen 200                                                                                                                               ✭
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r1 mono_ref.txt -nlen 500
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r1 mono_ref.txt -nlen 1000
    
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r2 bi_ref.txt  -nlen 200
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r2 bi_ref.txt  -nlen 500
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r2 bi_ref.txt  -nlen 1000
    
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r3 tri_ref.txt -nlen 200
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r3 tri_ref.txt -nlen 500
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r3 tri_ref.txt -nlen 1000
    
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r4 quad_ref.txt -nlen 200
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r4 quad_ref.txt -nlen 500
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r4 quad_ref.txt -nlen 1000
    20.148664764820452
    23.920623209756698
    23.920623209756698
    434.753653515586
    415.6565528488983
    415.6565528488983
    5648.123878741224
    5231.103323340586
    5231.103323340586
    49989.3929392947
    45365.80049320461
    45365.80049320461
    ```

* Szyfrogram dla różnych długości próbek

    ```
    ➜  Task04 (main) dotnet run --project Task04/Task04.csproj -- -e -i plaintext.txt -o ciphertext.txt -k key.txt                                                                                                                             ✭
    
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r1 mono_ref.txt -nlen 200
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r1 mono_ref.txt -nlen 500
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r1 mono_ref.txt -nlen 1000
    
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r2 bi_ref.txt  -nlen 200
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r2 bi_ref.txt  -nlen 500
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r2 bi_ref.txt  -nlen 1000
    
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r3 tri_ref.txt -nlen 200
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r3 tri_ref.txt -nlen 500
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r3 tri_ref.txt -nlen 1000
    
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r4 quad_ref.txt -nlen 200
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r4 quad_ref.txt -nlen 500
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r4 quad_ref.txt -nlen 1000
    2209.9569475273233
    3363.8014254193768
    3363.8014254193768
    ERROR: Reference base misses 4 n-grams present in text: QZ,JX,XK,QN
    ERROR: Reference base misses 7 n-grams present in text: QZ,JX,XK,QN,QD,ZQ,KZ
    ERROR: Reference base misses 7 n-grams present in text: QZ,JX,XK,QN,QD,ZQ,KZ
    ERROR: Reference base misses 68 n-grams present in text: TXH,XHG,HGF,GFQ,FQZ,QZO,FQJ,QJX,JXO,QUT, ...
    ERROR: Reference base misses 106 n-grams present in text: TXH,XHG,HGF,GFQ,FQZ,QZO,FQJ,QJX,JXO,QUT, ...
    ERROR: Reference base misses 106 n-grams present in text: TXH,XHG,HGF,GFQ,FQZ,QZO,FQJ,QJX,JXO,QUT, ...
    ERROR: Reference base misses 154 n-grams present in text: FETX,ETXH,TXHG,XHGF,HGFQ,GFQZ,FQZO,QZOD,ZODT,OFQJ, ...
    ERROR: Reference base misses 229 n-grams present in text: FETX,ETXH,TXHG,XHGF,HGFQ,GFQZ,FQZO,QZOD,ZODT,OFQJ, ...
    ERROR: Reference base misses 229 n-grams present in text: FETX,ETXH,TXHG,XHGF,HGFQ,GFQZ,FQZO,QZOD,ZODT,OFQJ, ...
    ```

* Wykluczenia dla każdego rzędu

    ```
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt  -s -r1 mono_ref.txt --exclude J,K,Q,X,Z -nlen 1000
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r1 mono_ref.txt --exclude J,K,Q,X,Z -nlen 1000
    
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt  -s -r2 bi_ref.txt  --exclude QJ,XZ,JQ,KQ,ZQ -nlen 1000
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r2 bi_ref.txt  --exclude QJ,XZ,JQ,KQ,ZQ -nlen 1000
    
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt  -s -r3 tri_ref.txt --exclude THE,AND,QQQ -nlen 1000
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r3 tri_ref.txt --exclude THE,AND,QQQ -nlen 1000
    
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt  -s -r4 quad_ref.txt --exclude THAT,THIS,QZXQ -nlen 1000
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r4 quad_ref.txt --exclude THAT,THIS,QZXQ -nlen 1000
    
    # 4) Próg oczekiwanej liczności (--minE)
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt  -s -r1 mono_ref.txt --minE 5 -nlen 5000
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r1 mono_ref.txt --minE 5 -nlen 5000
    
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt  -s -r2 bi_ref.txt  --minE 5 -nlen 5000
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r2 bi_ref.txt  --minE 5 -nlen 5000
    
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt  -s -r3 tri_ref.txt --minE 5 -nlen 5000
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r3 tri_ref.txt --minE 5 -nlen 5000
    
    dotnet run --project Task04/Task04.csproj -- -i plaintext.txt  -s -r4 quad_ref.txt --minE 5 -nlen 5000
    dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r4 quad_ref.txt --minE 5 -nlen 5000
    ```