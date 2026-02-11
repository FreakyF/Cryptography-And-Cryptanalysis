# Laboratory 6 | Geffe Generator & Correlation Cryptanalysis

A technical simulation framework implementing a statistical divide-and-conquer attack on a Geffe-style stream cipher composed of three Linear Feedback Shift Registers (LFSRs).

## 📺 Demo & Visuals
*Empirical benchmarks and execution logs.*

* **Correlation Attack Orchestration:**
```text
=== PHASE I: Key Generation ===
Original X: 100
Original Y: 1011
Original Z: 01000

=== PHASE II: Encryption ===
Plaintext: Coś tam coś tam kryptografia hehe.
Ciphertext (bits): 288 bits generated.

=== PHASE III: Keystream Recovery ===
Keystream recovered successfully.

=== PHASE IV: Correlation Attack ===
--- Starting Correlation Attack ---
Analyzing Register X...
Selected X: 100 with Rho=0.5415
Analyzing Register Z...
Selected Z: 01000 with Rho=0.4457
Recovering Register Y (Exhaustive search)...
Found Y: 1011

=== PHASE V: Verification ===
X: Orig=100 Rec=100 [OK]
Y: Orig=1011 Rec=1011 [OK]
Z: Orig=01000 Rec=01000 [OK]
Decrypted Text: Coś tam coś tam kryptografia hehe.
SUCCESS: Attack successful.
```

* **Bit-Level Persistence & Integrity (Binary diff):**
```text
➜  publish (main) diff secret_message.txt secret_message.enc                                                ✱
1c1
< Top secret data for files or something.
\ No newline at end of file
---
> 7OFzR5Z2L/[
\ No newline at end of file
➜  publish (main) diff secret_message.enc secret_message_dec.txt                                            ✱
1c1
< 7OFzR5Z2L/[
\ No newline at end of file
---
> Top secret data for files or something.
\ No newline at end of file
➜  publish (main) diff secret_message_dec.txt secret_message.txt
```

## 🏗️ Architecture & Context
*High-level system design and execution model.*

* **Objective:** Demonstration of the vulnerability of correlation-immune combiners by recovering internal LFSR states from a captured keystream without exhaustive search.
* **Architecture Pattern:** Domain-Driven Design (DDD) Lite, separating Domain logic (Generators/Attacks) from Application orchestration and Infrastructure utilities.
* **Data Flow:**
    1.  **Ingestion:** Conversion of plaintext or files into bit transformations.
    2.  **Encryption:** XOR stream cipher execution utilizing the Geffe combiner.
    3.  **Analysis:** Statistical evaluation using Pearson Rho correlation.
    4.  **Recovery:** Incremental state recovery based on probabilistic bias.

## ⚖️ Design Decisions & Trade-offs
*Technical justifications for architectural and algorithmic choices.*

* **Data Representation: Arithmetic-Optimized Bit Arrays**
    * **Context:** Internal representation of binary sequences for high-frequency LFSR operations and correlation calculations.
    * **Decision:** Utilization of `int[]` arrays to represent individual bits (0/1).
    * **Rationale:** Optimizes for arithmetic simplicity in Pearson calculations and array indexing during the development phase.
    * **Trade-off:** Prioritized development velocity and readability over memory efficiency, accepting a significant increase in memory footprint compared to packed bit representations.

* **Cryptanalysis Strategy: Statistical Divide-and-Conquer**
    * **Context:** Recovering initial states of three LFSRs with a total key space of $2^{L_x} \times 2^{L_y} \times 2^{L_z}$.
    * **Decision:** Implementation of a correlation attack targeting registers X and Z independently, followed by an exhaustive search for Y.
    * **Rationale:** Reduces computational complexity from exponential to linear relative to register count—approximately $O(2^{L_x} + 2^{L_z} + 2^{L_y})$.
    * **Trade-off:** Chose algorithmic speed over immediate precision, requiring a longer intercepted keystream to achieve statistical significance.

* **Experimentation: Deterministic Randomness Source**
    * **Context:** Initialization of internal LFSR states for simulation benchmarks.
    * **Decision:** Utilization of standard `System.Random` PRNG.
    * **Rationale:** Ensures deterministic reproducibility for `ExperimentRunner` benchmarks to validate attack success rates across trials.
    * **Trade-off:** Prioritized testability and reproducibility over cryptographic security for the purposes of the simulation.

## 🧠 Engineering Challenges
*Analysis of non-trivial technical hurdles and implemented solutions.*

* **Statistical Signal Extraction in Non-Linear Functions:**
    * **Problem:** The non-linear combining function $f(x, y, z)$ masks the direct output of individual registers, creating noise that obscures linear recurrence relations.
    * **Implementation:** Developed a Pearson Correlation Coefficient calculator to detect the 75% probabilistic bias where the keystream agrees with LFSR X and Z.
    * **Outcome:** Verifiable recovery of registers X and Z with high confidence (Correlation $\approx 0.5$), effectively filtering false positives through unicity distance analysis.

* **Bit-Level Persistence Infrastructure:**
    * **Problem:** Standard file systems operate on byte boundaries, while stream ciphers operate on continuous bitstreams, leading to potential alignment issues.
    * **Implementation:** Custom `BitUtils` infrastructure engineered to handle byte splitting and re-packing while maintaining MSB/LSB consistency.
    * **Outcome:** Lossless binary file encryption and decryption capable of processing arbitrary file types in `.enc` format.

## 🛠️ Tech Stack & Ecosystem
* **Core:** C# 14 / .NET 10.0
* **Persistence:** Stream-based Binary File System
* **Infrastructure:** Linux-x64 Console Application
* **Tooling:** Custom `ExperimentRunner` for Monte Carlo simulations

## 🧪 Quality & Standards
* **Testing Strategy:** Monte Carlo simulations executing 20 trials per keystream length to empirically determine attack success probability.
* **Observability:** Telemetry reporting Pearson Rho values, execution timing, and Hamming weight comparisons for state verification.
* **Engineering Principles:** Strong typing, explicit dependency injection, and strict separation of concerns between generator simulation and statistical analysis.

## 🙋‍♂️ Author

**Kamil Fudala**

- [GitHub](https://github.com/FreakyF)
- [LinkedIn](https://www.linkedin.com/in/kamil-fudala/)

## ⚖️ License

This project is licensed under the [MIT License](LICENSE).
