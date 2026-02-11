# Laboratory 1 | Substitution Cipher & Frequency Analysis

A technical implementation of monoalphabetic substitution operations paired with an n-gram frequency analysis engine for automated statistical cryptanalysis.

## 📺 Demo & Visuals
Empirical benchmarks and execution logs.

* **Substitution Cipher: Encryption Operations (Task 01):**
```text
➜  Task01 (main) dotnet run --project Task01/Task01.csproj -- -e -k key.txt -i plaintext.txt -o ciphertext.txt
➜  Task01 (main) diff plaintext.txt ciphertext.txt
1,4c1
< Once upon a time, in a quiet village surrounded by hills and forests, there lived a young boy who loved to read stories.
< He spent his evenings by the fireplace, turning the pages of old books filled with adventures and mysteries.
< The more he read, the more he dreamed of distant lands and brave heroes.
< One day, he decided that his own life should become a story worth telling.
---
> GFETXHGFQZODTOFQJXOTZCOSSQUTLXKKGXFRTRWNIOSSLQFRYGKTLZLZITKTSOCTRQNGXFUWGNVIGSGCTRZGKTQRLZGKOTLITLHTFZIOLTCTFOFULWNZITYOKTHSQETZXKFOFUZITHQUTLGYGSRWGGALYOSSTRVOZIQRCTFZXKTLQFRDNLZTKOTLZITDGKTITKTQRZITDGKTITRKTQDTRGYROLZQFZSQFRLQFRWKQCTITKGTLGFTRQNITRTEORTRZIQZIOLGVFSOYTLIGXSRWTEGDTQLZGKNVGKZIZTSSOFU
\ No newline at end of file
```

* **Substitution Cipher: Decryption & Reconstruction (Task 01):**
```text
➜  Task01 (main) dotnet run --project Task01/Task01.csproj -- -d -k key.txt -i ciphertext.txt -o recovered.txt
➜  Task01 (main) diff plaintext.txt recovered.txt
1,4c1
< Once upon a time, in a quiet village surrounded by hills and forests, there lived a young boy who loved to read stories.
< He spent his evenings by the fireplace, turning the pages of old books filled with adventures and mysteries.
< The more he read, the more he dreamed of distant lands and brave heroes.
< One day, he decided that his own life should become a story worth telling.
---
> ONCEUPONATIMEINAQUIETVILLAGESURROUNDEDBYHILLSANDFORESTSTHERELIVEDAYOUNGBOYWHOLOVEDTOREADSTORIESHESPENTHISEVENINGSBYTHEFIREPLACETURNINGTHEPAGESOFOLDBOOKSFILLEDWITHADVENTURESANDMYSTERIESTHEMOREHEREADTHEMOREHEDREAMEDOFDISTANTLANDSANDBRAVEHEROESONEDAYHEDECIDEDTHATHISOWNLIFESHOULDBECOMEASTORYWORTHTELLING
\ No newline at end of file
```

* **Automated N-Gram Frequency Distribution (Task 02):**
```text
➜  Task02 (main) dotnet run --project Task02/Task02.csproj -- -i plaintext.txt -g1 monograms.txt
➜  Task02 (main) cat monograms.txt | tail -n 10
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

➜  Task02 (main) dotnet run --project Task02/Task02.csproj -- -i plaintext.txt -g2 bigrams.txt
➜  Task02 (main) cat bigrams.txt | tail -n 10
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

➜  Task02 (main) dotnet run --project Task02/Task02.csproj -- -i plaintext.txt -g3 trigrams.txt
➜  Task02 (main) cat trigrams.txt | tail -n 10
WIT 1
WNL 1
WOR 1
YHE 1
YHI 1
YOU 1
YST 1
YTH 1
YWH 1
YWO 1

➜  Task02 (main) dotnet run --project Task02/Task02.csproj -- -i plaintext.txt -g4 quadgrams.txt
➜  Task02 (main) cat quadgrams.txt | tail -n 10
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

* **Linguistic Reference Base Construction (Task 03/04):**
```text
➜  Task03 (main) dotnet run --project Task03/Task03.csproj -- -i corpus.txt -b1 mono_ref.txt -b2 bi_ref.txt -b3 tri_ref.txt -b4 quad_ref.txt
```

* **Chi-Square Statistical Distance: Plaintext (Task 03/04):**
```text
➜  Task03 (main) dotnet run --project Task03/Task03.csproj -- -i plaintext.txt -s -r1 mono_ref.txt
23.920623209756698

➜  Task03 (main) dotnet run --project Task03/Task03.csproj -- -i plaintext.txt -s -r2 bi_ref.txt
415.6565528488983

➜  Task03 (main) dotnet run --project Task03/Task03.csproj -- -i plaintext.txt -s -r3 tri_ref.txt
5231.103323340586

➜  Task03 (main) dotnet run --project Task03/Task03.csproj -- -i plaintext.txt -s -r4 quad_ref.txt
45365.80049320461
```

* **Chi-Square Statistical Distance: Ciphertext (Task 04):**
```text
➜ Task04 (main) dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r1 mono_ref.txt -nlen 200 ✭
dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r1 mono_ref.txt -nlen 500
dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r1 mono_ref.txt -nlen 1000

dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r2 bi_ref.txt -nlen 200
dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r2 bi_ref.txt -nlen 500
dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r2 bi_ref.txt -nlen 1000

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

* **Advanced Cryptanalytic Filtering (Task 04):**
```text
➜ Task04 (main) dotnet run --project Task04/Task04.csproj -- -e -i plaintext.txt -o ciphertext.txt -k key.txt ✭

dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r1 mono_ref.txt -nlen 200
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r1 mono_ref.txt -nlen 500
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r1 mono_ref.txt -nlen 1000

dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r2 bi_ref.txt -nlen 200
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r2 bi_ref.txt -nlen 500
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r2 bi_ref.txt -nlen 1000

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

dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r1 mono_ref.txt --exclude J,K,Q,X,Z -nlen 1000
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r1 mono_ref.txt --exclude J,K,Q,X,Z -nlen 1000

dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r2 bi_ref.txt --exclude QJ,XZ,JQ,KQ,ZQ -nlen 1000
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r2 bi_ref.txt --exclude QJ,XZ,JQ,KQ,ZQ -nlen 1000

dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r3 tri_ref.txt --exclude THE,AND,QQQ -nlen 1000
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r3 tri_ref.txt --exclude THE,AND,QQQ -nlen 1000

dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r4 quad_ref.txt --exclude THAT,THIS,QZXQ -nlen 1000
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r4 quad_ref.txt --exclude THAT,THIS,QZXQ -nlen 1000

# 4) Próg oczekiwanej liczności (--minE)

dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r1 mono_ref.txt --minE 5 -nlen 5000
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r1 mono_ref.txt --minE 5 -nlen 5000

dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r2 bi_ref.txt --minE 5 -nlen 5000
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r2 bi_ref.txt --minE 5 -nlen 5000

dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r3 tri_ref.txt --minE 5 -nlen 5000
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r3 tri_ref.txt --minE 5 -nlen 5000

dotnet run --project Task04/Task04.csproj -- -i plaintext.txt -s -r4 quad_ref.txt --minE 5 -nlen 5000
dotnet run --project Task04/Task04.csproj -- -i ciphertext.txt -s -r4 quad_ref.txt --minE 5 -nlen 5000
```

## 🏗️ Architecture & Context
*High-level system design and execution model.*

* **Objective:** Provision of a robust CLI toolchain for the encryption, decryption, and statistical evaluation of text using substitution ciphers.
* **Architecture Pattern:** Layered Architecture (Clean Architecture), enforcing strict separation between Domain logic, Application use cases, and Infrastructure adapters.
* **Data Flow:** CLI Arguments → `CommandLineOptionsProvider` → `AppOptions` → `Validator` → `Runner` → Domain Services (`Cipher`, `NGramCounter`, `ChiSquare`) → `FileWriter`.

## ⚖️ Design Decisions & Trade-offs
*Technical justifications for architectural and implementation choices.*

* **Persistence: Atomic Write Operations**
    * **Context:** Requirement for ensuring data integrity during file output.
    * **Decision:** Implementation of atomic writes (writing to temporary files before moving to the target destination).
    * **Rationale:** This prevents file corruption or partial writes in the event of process interruption.
    * **Trade-off:** Accepted increased write latency and I/O operations in exchange for higher data durability and consistency.

* **State Management: In-Memory Text Processing**
    * **Context:** Analysis of large text files for n-gram distributions.
    * **Decision:** Utilization of full in-memory processing via `File.ReadAllText`.
    * **Rationale:** Simplifies the implementation of sliding window algorithms and text normalization logic.
    * **Trade-off:** System scalability is bounded by available RAM, prioritizing code maintainability over the ability to process extremely large files.

* **Execution Model: Synchronous Single-Threading**
    * **Context:** Execution of cryptographic analysis on single-file workloads.
    * **Decision:** Adoption of a synchronous execution model.
    * **Rationale:** Minimizes context-switching overhead and synchronization complexity for standard script-based workloads.
    * **Trade-off:** Sacrificed multi-core utilization for predictable execution order and simplified debugging.

## 🧠 Engineering Challenges
*Analysis of non-trivial technical hurdles and implemented solutions.*

* **Statistical Divergence in Chi-Square Calculations:**
    * **Problem:** High variance in statistics due to short sample texts or rare n-grams skewing the cryptanalytic results.
    * **Implementation:** Introduction of a configurable `ChiSquareOptions` model, supporting `MinExpected` thresholds and explicit `Exclusion` sets to filter noise.
    * **Outcome:** Achievement of stable and accurate statistical distances, significantly reducing false positives during reference fitting.

* **Substitution Key Integrity:**
    * **Problem:** Malformed keys (e.g., duplicate targets or missing source characters) rendering the ciphertext irreversible.
    * **Implementation:** Enforcement of strict bijection validation in the `KeyLoader`, verifying a perfect 1-to-1 mapping for the 26-character Latin alphabet.
    * **Outcome:** Guaranteed reversibility of all encryption operations, preventing data loss caused by key inconsistencies.

## 🛠️ Tech Stack & Ecosystem
* **Core:** C# / .NET 9.0
* **Persistence:** Local Filesystem (Atomic writes)
* **Infrastructure:** Microsoft.Extensions.Configuration.CommandLine
* **Tooling:** JetBrains.Annotations

## 🧪 Quality & Standards
* **Testing Strategy:** Implementation of component-based testing through interface abstractions such as `ISubstitutionCipher` and `INGramCounter`.
* **Observability:** Standard Error (stderr) reporting for validation failures with distinct exit codes (0: Success, 1: Runtime Error, 2: Validation Error).
* **Engineering Principles:** Adherence to SOLID principles, Dependency Injection, and the use of Immutable Domain Models.

## 🙋‍♂️ Author

**Kamil Fudala**

- [GitHub](https://github.com/FreakyF)
- [LinkedIn](https://www.linkedin.com/in/kamil-fudala/)

## ⚖️ License

This project is licensed under the [MIT License](LICENSE).
