# Kryptografia i kryptoanaliza

## Laboratorium 2

### Grupa 1ID24B

### Autorzy: Kamil Fudala, Andrzej Szczytyński

### Zadanie 1

Napisz program implementujący algorytm szyfru przesuwnego (Szyfr Cezara).

1. Tekst jawny powinien być importowany do programu z pliku tekstowego, którego nazwa określona powinna być po zdefiniowanym argumencie / fladze: -i.
2. Wynik pracy programu powinien być eksportowany do pliku tekstowego, którego nazwa określona powinna być po zdefiniowanym argumencie / fladze: -o.
3. Klucz powinien być określany za pomocą parametru / flagi -k.
4. Tryb pracy programu powinien być określony poprzez flagi: -e dla procesu szyfrowania, -d dla procesu deszyfrowania.

#### Diagram przepływu danych

```mermaid
%%{init: {
  "flowchart": { "curve":"step", "nodeSpacing": 120, "rankSpacing": 100 }
}}%%
flowchart TD
    user[["CLI User"]]
    provider["CommandLineOptionsProvider\n(TryGetOptions)"]
    validators["Options Validators\nAppOptionsValidator + FileSystemOptionsValidator"]
    printer["Printer\n(Usage / Errors)"]
    runner["Runner"]
    reader["FileReader"]
    normalizer["TextNormalizer"]
    keyLoader["KeyLoader"]
    cipher["SubstitutionCipher"]
    ngrams["NGramCounter"]
    report["NGramReportBuilder"]
    writer["FileWriter"]
    inputFile[("Input file")]
    keyFile[("Key file")]
    outputFile[("Output file")]
    reports[("N-gram reports")]

    user --> provider --> validators
    validators -- errors --> printer
    validators -- valid options --> runner

    runner --> reader
    reader --> inputFile
    inputFile --> reader
    reader --> normalizer --> runner

    runner --> keyLoader
    keyLoader --> keyFile
    keyFile --> keyLoader
    keyLoader --> cipher

    normalizer --> cipher
    cipher --> writer

    normalizer --> ngrams --> report --> writer

    writer --> outputFile
    writer --> reports
    runner -. status .-> printer
```

#### Implementacja

#### Wyniki

### Zadanie 2

Rozbuduj program z poprzedniego zadania poprzez implementację ataku typu brute-force na szyfrogram wygenerowany przy pomocy algorytmu przesuwnego.

1. Algorytm powinien być wyzwalany po użyciu flagi -a z parametrem bf.

#### Diagram przepływu danych

```mermaid
%%{init: {
  "flowchart": { "curve":"step", "nodeSpacing": 120, "rankSpacing": 100 }
}}%%
flowchart TD
    user[["CLI User"]]
    parser["ArgumentParser"]
    orchestrator["CipherOrchestrator"]
    fileService["FileService"]
    keyService["KeyService"]
    normalizer["TextNormalizer"]
    cipher["CaesarCipher"]
    brute["BruteForceAttack\n+ ChiSquareScorer"]
    inputFile[("Input file")]
    keyFile[("Key file")]
    outputFile[("Output file")]
    console["Console.Error"]

    user --> parser --> orchestrator

    orchestrator --> fileService
    fileService --> inputFile
    inputFile --> fileService
    fileService --> orchestrator

    orchestrator --> normalizer
    normalizer --> orchestrator

    orchestrator --> keyService
    keyService --> keyFile
    keyFile --> keyService
    keyService --> orchestrator

    orchestrator --> cipher
    cipher --> orchestrator

    orchestrator --> brute
    brute --> cipher
    brute --> orchestrator

    orchestrator --> fileService
    fileService --> outputFile
    orchestrator --> console
```

#### Implementacja

#### Wyniki

### Zadanie 3

Napisz program analogiczny do programu z zadania 1, który tym razem implementuje szyfr afiniczny.

#### Diagram przepływu danych

```mermaid
%%{init: {
  "flowchart": { "curve":"step", "nodeSpacing": 120, "rankSpacing": 100 }
}}%%
flowchart TD
    user[["CLI User"]]
    parser["ArgumentParser"]
    orchestrator["AffineCipherOrchestrator"]
    fileService["FileService"]
    keyService["KeyService"]
    normalizer["TextNormalizer"]
    cipher["AffineCipher"]
    inputFile[("Input file")]
    keyFile[("Key file")]
    outputFile[("Output file")]

    user --> parser --> orchestrator

    orchestrator --> fileService
    fileService --> inputFile
    inputFile --> fileService
    fileService --> orchestrator

    orchestrator --> normalizer --> orchestrator

    orchestrator --> keyService
    keyService --> keyFile
    keyFile --> keyService
    keyService --> orchestrator

    orchestrator --> cipher
    cipher --> orchestrator

    orchestrator --> fileService
    fileService --> outputFile
```

#### Implementacja

#### Wyniki

### Zadanie 4

Rozbuduj program z poprzedniego zadania poprzez implementację ataku typu brute-force na szyfrogram zaimplementowany przy pomocy szyfru afinicznego. Sposób pracy z programem powinien być analogiczny do pracy z programem z zadania 2.

#### Diagram przepływu danych

```mermaid
%%{init: {
  "flowchart": { "curve":"step", "nodeSpacing": 120, "rankSpacing": 100 }
}}%%
flowchart TD
    user[["CLI User"]]
    parser["ArgumentParser"]
    orchestrator["AffineCipherOrchestrator"]
    fileService["FileService"]
    keyService["KeyService"]
    normalizer["TextNormalizer"]
    cipher["AffineCipher"]
    brute["BruteForceAttack\n+ ChiSquareScorer"]
    inputFile[("Input file")]
    keyFile[("Key file")]
    outputFile[("Output file")]
    console["Console.Error"]

    user --> parser --> orchestrator

    orchestrator --> fileService
    fileService --> inputFile
    inputFile --> fileService
    fileService --> orchestrator

    orchestrator --> normalizer --> orchestrator

    orchestrator --> keyService
    keyService --> keyFile
    keyFile --> keyService
    keyService --> orchestrator

    orchestrator --> cipher
    cipher --> orchestrator

    orchestrator --> brute
    brute --> cipher
    brute --> orchestrator

    orchestrator --> fileService
    fileService --> outputFile
    orchestrator --> console
```

#### Implementacja

#### Wyniki
