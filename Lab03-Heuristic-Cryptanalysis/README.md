# Laboratory 3 | Substitution Cipher & Heuristic Cryptanalysis

A high-performance implementation for monoalphabetic substitution operations utilizing stochastic search algorithms and language modeling for automated key recovery.

## 📺 Demo & Visuals
Empirical benchmarks and execution logs.

* **Symmetric Operation & File Persistence (Task 01):**
```text
➜  Task01 (main) dotnet run -- \`                       ✭
  -e \
  -i Samples/plaintext.txt \
  -o Samples/cipher.txt

➜  Task01 (main) diff Samples/plaintext.txt  Samples/cipher.txt                                             ✭
1c1,2
< For several years I’ve lived
\ No newline at end of file
---
> ZMNETSIJHBKPLWVFDAOCURYQXG
> SVA
\ No newline at end of file
```

* **Heuristic Key Recovery Verification (Task 02 & 03):**
```text
➜  publish (main) ./Task03 -d -i Samples/cipher.txt -o Samples/output_key.txt -r Samples/bigrams.txt        ✭

➜  publish (main) diff <(head -c 100 Samples/cipher.txt) <(head -c 100 Samples/lotr.txt)                    ✭
1c1
< LSARNUKJAXVXUZRULNUGRAKNDJXASPQUJXNSUYKPVXGNSUKUQJOUGASPBBJNXPNAXAINDGJKNDMUNSPQUCJQQUGMJNSNSUUXGIPC
\ No newline at end of file
---
> CHAPTERIANUNEXPECTEDPARTYINAHOLEINTHEGROUNDTHERELIVEDAHOBBITNOTANASTYDIRTYWETHOLEFILLEDWITHTHEENDSOF
\ No newline at end of file
➜  publish (main)
```

* **Comparative Quality & Performance Analysis (Task 04):**
```text
Quality comparison
╭─────────────┬───────────────┬──────────────╮
│ Algorithm   │ Text accuracy │ Key accuracy │
├─────────────┼───────────────┼──────────────┤
│ MH (Task02) │ 100.00 %      │ 100.00 %     │
│ SA (Task03) │ 100.00 %      │ 100.00 %     │
╰─────────────┴───────────────┴──────────────╯
Performance comparison
╭─────────────┬───────────────────┬──────────────────────────┬───────────────────╮
│ Algorithm   │ Min iters for 85% │ Mean time (ms) - 10 runs │ Mean text acc (%) │
├─────────────┼───────────────────┼──────────────────────────┼───────────────────┤
│ MH (Task02) │ 4388              │ 0.550                    │ 87.45             │
│ SA (Task03) │ 1249              │ 1.121                    │ 92.37             │
╰─────────────┴───────────────────┴──────────────────────────┴───────────────────╯
╰─────────────┴───────────────────┴──────────────────────────┴───────────────────╯
```

* **Statistical Convergence Visualization (ASCII Charts):**
```text
Convergence analysis
╭─────────────────────────────────────╮                                      ╭─────────────────────────────────────╮
│ Convergence – objective — MH (mean) │                                      │ Convergence – objective — SA (mean) │
│  100114.54 ┤            ╭──╮  ╭     │                                      │  105898.94 ┤          ╭────────     │
│   93772.86 ┤          ╭─╯  ╰──╯     │                                      │   99681.76 ┤         ╭╯             │
│   87431.17 ┤         ╭╯             │                                      │   93464.57 ┤        ╭╯              │
│   81089.49 ┤         │              │                                      │   87247.38 ┤        │               │
│   74747.81 ┤        ╭╯              │                                      │   81030.19 ┤       ╭╯               │
│   68406.13 ┤        │               │                                      │   74813.00 ┤       │                │
│   62064.45 ┤       ╭╯               │                                      │   68595.81 ┤      ╭╯                │
│   55722.77 ┤      ╭╯                │                                      │   62378.62 ┤      │                 │
│   49381.09 ┤      │                 │                                      │   56161.43 ┤     ╭╯                 │
│   43039.40 ┤    ╭─╯                 │                                      │   49944.25 ┤    ╭╯                  │
│   36697.72 ┤    │                   │                                      │   43727.06 ┤   ╭╯                   │
│   30356.04 ┤  ╭─╯                   │                                      │   37509.87 ┤ ╭─╯                    │
│   24014.36 ┼──╯                     │                                      │   31292.68 ┼─╯                      │
│ iters: 1         1000     500000    │                                      │ iters: 1         1000     500000    │
╰─────────────────────────────────────╯                                      ╰─────────────────────────────────────╯
╭─────────────────────────────────────────────╮                              ╭─────────────────────────────────────────────╮
│ Convergence – text accuracy (%) — MH (mean) │                              │ Convergence – text accuracy (%) — SA (mean) │
│  89.49 ┤              ╭╮                    │                              │  100.00 ┤          ╭────╮╭──                │
│  82.39 ┤           ╭──╯╰───                 │                              │   91.91 ┤         ╭╯    ╰╯                  │
│  75.28 ┤          ╭╯                        │                              │   83.82 ┤         │                         │
│  68.18 ┤         ╭╯                         │                              │   75.73 ┤         │                         │
│  61.07 ┤         │                          │                              │   67.64 ┤        ╭╯                         │
│  53.97 ┤         │                          │                              │   59.55 ┤        │                          │
│  46.86 ┤         │                          │                              │   51.46 ┤        │                          │
│  39.76 ┤        ╭╯                          │                              │   43.37 ┤       ╭╯                          │
│  32.65 ┤        │                           │                              │   35.28 ┤       │                           │
│  25.55 ┤        │                           │                              │   27.19 ┤      ╭╯                           │
│  18.44 ┤      ╭─╯                           │                              │   19.10 ┤    ╭╮│                            │
│  11.34 ┤      │                             │                              │   11.01 ┤ ╭──╯╰╯                            │
│   4.23 ┼──────╯                             │                              │    2.92 ┼─╯                                 │
│ iters: 1        1000   500000               │                              │ iters: 1        1000   500000               │
╰─────────────────────────────────────────────╯                              ╰─────────────────────────────────────────────╯
```


## 🏗️ Architecture & Context
*High-level system design and execution model.*

* **Objective:** Automation of substitution cipher decryption without known keys through the application of statistical language models.
* **Architecture Pattern:** Clean Architecture, enforcing strict segregation between Application, Domain, and Infrastructure layers.
* **Data Flow:** Raw Text Input → Normalization Engine → Heuristic Analyzer (MCMC/SA) → Bigram Scoring → Key Recovery → Output.

## ⚖️ Design Decisions & Trade-offs
*Technical justifications for architectural and implementation choices.*

* **Performance Strategy: Unmanaged Memory Access**
    * **Context:** The scoring function constitutes the critical hot path, executed millions of times during analysis.
    * **Decision:** Utilization of `Unsafe.Add` and `ref` pointers for array access within the `BigramLanguageModel`.
    * **Rationale:** Removal of bounds-checking overhead facilitates maximum throughput during the iterative search phase.
    * **Trade-off:** Sacrificed managed memory safety guarantees for a significant reduction in execution time—achieving a **99.5% reduction (from 100s to 519ms)**.

* **Algorithmic Efficiency: Incremental Score Updates**
    * **Context:** Modifying the key permutation involves swapping two elements, which typically triggers a full recomputation of text fitness.
    * **Decision:** Implementation of `ProposedScoreDelta` to calculate log-likelihood changes based exclusively on affected bigrams.
    * **Rationale:** Reduces computational complexity from $O(L)$ (text length) to $O(1)$ relative to the alphabet size.
    * **Trade-off:** Increased implementation complexity in the update logic to achieve constant-time scoring updates.

* **Benchmarking: Process-Isolated Execution**
    * **Context:** Performance comparison between diverse heuristic solvers.
    * **Decision:** Execution of solvers as external processes via a dedicated `ProcessRunner`.
    * **Rationale:** Ensures the benchmarking harness measures actual startup and runtime characteristics of Native AOT-compiled binaries rather than JIT-warmed code.
    * **Trade-off:** Accepted higher latency in test orchestration for more realistic and reproducible performance metrics.

## 🧠 Engineering Challenges
*Analysis of non-trivial technical hurdles and implemented solutions.*

* **Stochastic Convergence Stability:**
    * **Problem:** Standard hill-climbing algorithms frequently stagnate in local maxima, failing to recover the global optimum (the correct key).
    * **Implementation:** Integration of the Metropolis-Hastings algorithm and Simulated Annealing with reheating cycles.
    * **Outcome:** The implementation effectively escapes local optima, consistently converging on the global maximum given sufficient iterations.

* **Efficient PRNG State Management:**
    * **Problem:** `System.Random` is computationally expensive and not thread-safe for high-frequency sampling in tight loops.
    * **Implementation:** Porting of the `xoshiro256**` PRNG algorithm for low-overhead, high-quality randomness.
    * **Outcome:** Enabled intensive Monte Carlo simulations with minimal computational latency.

## 🛠️ Tech Stack & Ecosystem
* **Core:** C# 12 / .NET 9
* **Infrastructure:** Native AOT (Ahead-of-Time) Compilation
* **Tooling:** Spectre.Console, AsciiChart.Sharp

## 🧪 Quality & Standards
* **Testing Strategy:** Black-box differential testing utilized through a Task04 comparison orchestrator.
* **Observability:** Console-based reporting with ASCII visualization of convergence series for real-time monitoring of algorithm behavior.
* **Engineering Principles:** Zero-allocation in hot paths, aggressive inlining for performance optimization, and adherence to SOLID architecture.

## 🙋‍♂️ Author

**Kamil Fudala**

- [GitHub](https://github.com/FreakyF)
- [LinkedIn](https://www.linkedin.com/in/kamil-fudala/)

## ⚖️ License

This project is licensed under the [MIT License](LICENSE).
