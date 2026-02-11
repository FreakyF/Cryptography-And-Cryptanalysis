# Laboratory 7 | Trivium Cipher & Algebraic Cryptanalysis

A high-performance cryptographic engine designed for the Trivium stream cipher, utilizing SIMD vectorization and unmanaged memory access to achieve near-wire speeds, coupled with an algebraic Cube Attack solver.

## 📺 Demo & Visuals
*Empirical benchmarks and execution logs.*

* **Core Algorithm Verification:**
```text
--- Experiment 1: Verification ---`
Generated: FBE0BF265859051B517A2E4E239FC97F563203161907CF2DE7A8790FA1B2E9CD
Expected:  FBE0BF265859051B517A2E4E239FC97F563203161907CF2DE7A8790FA1B2E9CD
Match:     True
Involutive Check (Dec(Enc(P)) == P): True
```

* **IV Reuse Attack (Known-Plaintext Recovery):**
```text
--- Experiment 2: IV Reuse Attack ---
Encryption took: 7.10 μs
Recovered P2: HTTP/1.1 404 Not Found
From: admin@local

Data: 67890
Crib 'HTTP': 42 matches in 1.30 μs
Crib 'Content-Type:': 13 matches in 2.30 μs
Crib 'Secret:': 29 matches in 1.30 μs
Crib '200 OK': 37 matches in 0.90 μs
```

* **Throughput & State Evolution Analysis:**

```text
--- Experiment 3: Rounds Analysis Performance & State Evolution ---
Rounds   | Ones   | Balance  | Chi-Sq     | Warmup (μs)  | Throughput
0        | 83     | 0.288    | 2.25       | 0.00         | 8816.48      Mbps
192      | 154    | 0.535    | 3.10       | 0.00         | 8661.83      Mbps
288      | 156    | 0.542    | 3.31       | 0.00         | 7854.10      Mbps
384      | 142    | 0.493    | 3.61       | 0.00         | 8762.55      Mbps
480      | 149    | 0.517    | 3.69       | 0.00         | 8773.32      Mbps
576      | 149    | 0.517    | 3.31       | 0.00         | 8614.97      Mbps
768      | 143    | 0.497    | 4.00       | 0.00         | 8703.98      Mbps
1152     | 154    | 0.535    | 3.69       | 0.00         | 8719.84      Mbps
```

* **Algebraic Cube Attack (Reduced Rounds Recovery):**
```text
--- Experiment 4: Cube Attack (Reduced Versions) ---
Size 1: 1 cubes in 260.90 μs
Size 2: 6 cubes in 376.30 μs
Size 3: 2 cubes in 545.00 μs
Size 4: 5 cubes in 942.40 μs
Size 5: 2 cubes in 1693.30 μs
Size 6: 1 cubes in 3494.10 μs
Online phase: recovery took 262.20 μs
Rounds: 192 | Found: 17 | Accuracy: 70.6 % | Offline: 7345.40 μs
Size 1: 7 cubes in 210.70 μs
Size 2: 4 cubes in 308.90 μs
Size 3: 3 cubes in 478.00 μs
Size 4: 5 cubes in 814.70 μs
Size 5: 5 cubes in 1429.30 μs
Size 6: 4 cubes in 2945.20 μs
Online phase: recovery took 160.70 μs
Rounds: 288 | Found: 28 | Accuracy: 64.3 % | Offline: 6220.60 μs
Size 1: 5 cubes in 210.00 μs
Size 2: 4 cubes in 324.80 μs
Size 3: 4 cubes in 478.70 μs
Size 4: 6 cubes in 814.70 μs
Size 5: 1 cubes in 1453.20 μs
Size 6: 5 cubes in 2965.80 μs
Online phase: recovery took 115.40 μs
Rounds: 384 | Found: 25 | Accuracy: 44.0 % | Offline: 6275.90 μs
Size 1: 5 cubes in 211.40 μs
Size 2: 4 cubes in 311.50 μs
Size 3: 2 cubes in 494.80 μs
Size 4: 4 cubes in 806.60 μs
Size 5: 8 cubes in 1499.70 μs
Size 6: 2 cubes in 3315.20 μs
Online phase: recovery took 90.50 μs
Rounds: 480 | Found: 25 | Accuracy: 36.0 % | Offline: 6667.10 μs
```

* **Statistical Reliability Testing:**
```text
--- Experiment 5: Statistical Comparison ---
Testing rounds: 0
Length: 1000000
Frequency (Ones): 50.03 % (Exp: 50%)
Runs: 499359 (Exp: 500001)
Autocorrelation (Lag 1): 0.0013 (Exp: < 0.1)
Chi-Square Statistic: 0.4651 (Critical Value α=0.05: 3.841)
Testing rounds: 288
Length: 1000000
Frequency (Ones): 50.03 % (Exp: 50%)
Runs: 499369 (Exp: 500001)
Autocorrelation (Lag 1): 0.0013 (Exp: < 0.1)
Chi-Square Statistic: 0.4624 (Critical Value α=0.05: 3.841)
Testing rounds: 1152
Length: 1000000
Frequency (Ones): 50.03 % (Exp: 50%)
Runs: 499382 (Exp: 500001)
Autocorrelation (Lag 1): 0.0012 (Exp: < 0.1)
Chi-Square Statistic: 0.4462 (Critical Value α=0.05: 3.841)
```

* **High-Throughput Saturation (SIMD AVX2):**
```text
--- Experiment 6: 1 Billion Bits Saturation Test (Fixed) ---
Scale: 1,000,000,000 bits (1 Gbit)
Keystream (byte[])   | Time: 642.32 ms | Speed: 1556.86 Mbps
Encryption (byte[])  | Speed: 70.05 Gbps
Integrity Check: True
```

## 🏗️ Architecture & Context
*High-level system design optimizations.*

* **Objective:** Achievement of near-wire encryption speed and execution of algebraic key recovery on reduced-round Trivium variants.
* **Architecture Pattern:** Data-Oriented Design (DOD) featuring monolithic compute kernels for maximum throughput.
* **Data Flow:**
    1.  **Configuration:** Initialization of Key/IV states.
    2.  **Expansion:** SIMD state expansion for parallel processing.
    3.  **Generation:** Parallel block generation using AVX2 intrinsics.
    4.  **Mutation:** High-speed buffer mutation via unsafe pointers.
    5.  **Analysis:** Data streaming to statistical or algebraic cryptanalysis sinks.

## ⚖️ Design Decisions & Trade-offs
*Technical justifications for architectural and performance choices.*

* **Concurrency Strategy: Multi-Stream AVX2 Parallelization**
    * **Context:** Trivium state updates involve non-byte-aligned bit taps, complicating vectorization due to data dependencies.
    * **Decision:** Implementation of a multi-stream parallelization strategy utilizing `Vector256<ulong>` to process 8 independent streams simultaneously.
    * **Rationale:** Maximizes CPU port utilization and memory bandwidth saturation on modern x64 processors by breaking the sequential dependency chain.
    * **Trade-off:** Prioritized aggregate throughput over single-stream latency, requiring a fallback to scalar execution for small payloads (<128KB).

* **Memory Management: Unmanaged Pointer Arithmetic**
    * **Context:** High-frequency state updates in the execution hot loop create significant garbage collection pressure and array bounds-checking overhead.
    * **Decision:** Utilization of unsafe pointer arithmetic and stack allocation (`stackalloc`, `fixed`).
    * **Rationale:** Eliminates JIT-induced bounds checks and ensures data remains pinned in L1/L2 cache during heavy processing.
    * **Trade-off:** Chose raw execution speed over managed memory safety, accepting the risk of buffer overflows and loss of runtime verifiability.

* **Algebraic Solver: Fixed-Width Bit-Packed Gaussian Elimination**
    * **Context:** The online phase of a Cube Attack requires solving systems of linear equations over $GF(2)$.
    * **Decision:** Implementation of a Gaussian Elimination solver utilizing 128-bit fixed-width rows.
    * **Rationale:** Enables processing 64 coefficients per CPU cycle using native 64-bit XOR operations, significantly outperforming generic matrix implementations.
    * **Trade-off:** Prioritized solver latency optimization over flexibility, accepting a hard limit of 127 variables per system.

## 🧠 Engineering Challenges
*Analysis of non-trivial technical hurdles and implemented solutions.*

* **Low-Overhead State Serialization:**
    * **Problem:** Mapping the 288-bit Trivium state across three registers into 64-bit words results in expensive bit-shifting operations during state updates.
    * **Implementation:** Engineered a custom bit-mapping strategy where Key/IV bits are loaded in reverse order, aligning shift directions with native CPU instruction sets (`SHR`/`SHL`).
    * **Outcome:** Successfully removed conditional branching from the state update function, allowing the compiler to emit branchless assembly.

[Image of Trivium stream cipher internal state registers]

* **SIMD Register Pressure Management:**
    * **Problem:** Maintaining 8 parallel Trivium states requires significantly more registers than available in standard x64 calling conventions, risking register spilling to the stack.
    * **Implementation:** Utilized aggressive manual loop unrolling (depth 4) within the AVX2 block and explicit `Sse.Prefetch0` instructions.
    * **Outcome:** Forced the compiler to maintain critical state variables in YMM registers for the duration of batch processing—**reaching peak throughput of 70 Gbps**.

## 🛠️ Tech Stack & Ecosystem
* **Core:** C# 14 / .NET 10.0
* **Compilation:** Native AOT with `avx2` and `bmi2` instruction set optimization
* **Infrastructure:** Linux x64 with HugePages (`vm.nr_hugepages`) enabled
* **Tooling:** `System.Runtime.Intrinsics` and unmanaged compiler services

## 🧪 Quality & Standards
* **Testing Strategy:** Known Answer Tests (KAT) verified against Trivium specifications and involutive property checks ($P = D(E(P))$).
* **Observability:** Microsecond-precision telemetry utilizing `Stopwatch` for warmup, throughput, and offline analysis phases.
* **Engineering Principles:** Zero-allocation on hot paths, branch prediction optimization, and L1/L2 cache locality maximization.

## 🙋‍♂️ Author

**Kamil Fudala**

- [GitHub](https://github.com/FreakyF)
- [LinkedIn](https://www.linkedin.com/in/kamil-fudala/)

## ⚖️ License

This project is licensed under the [MIT License](LICENSE).
