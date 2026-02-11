# Laboratory 4 | LCG Stream Cipher & Algebraic Cryptanalysis

A technical implementation of a Linear Congruential Generator (LCG) and an automated known-plaintext attack engine designed to reconstruct internal parameters through modular arithmetic.

## 📺 Demo & Visuals
Empirical benchmarks and execution logs.

* **Phase I-VI: Full Parameter Recovery Lifecycle:**
```text
Phase I: Cipher parameter generation
State bit length n: 100
Modulus m: 1267650600228229401496703205361
Parameter A: 507137210131719328049569126076
Parameter B: 209237662642201117423903484764
Seed S0: 1121370110926348445843314945442

Phase II: Message encryption
Plaintext length in characters: 180
Ciphertext first 100 bits: 0011100010001101101010100000110100111110001111011001111110000010000000001011010011000111010110101100

Phase III: Cryptanalytic known-plaintext attack
Known plaintext length in characters: 38
Known plaintext length in bits: 304
Required bits for three states (3n): 300
Assumption: the attacker knows the first 304 bits of the keystream.
Known plaintext X_k: Linear congruential generators are not

Phase III result: parameters recovered successfully.
Recovered A*: 507137210131719328049569126076
Recovered B*: 209237662642201117423903484764
gcd(mu, m): 1
The solution is unique because gcd(mu, m) = 1.

Phase IV: Verification of recovered parameters
A matches original: True
B matches original: True

Phase V: Recovery of initial state
Recovered S0*: 1121370110926348445843314945442
Seed matches original: True

Phase VI: Decryption of full message
Decrypted plaintext:
Linear congruential generators are not secure for cryptographic purposes. This demonstration shows how a known-plaintext attack fully recovers the key stream and breaks the cipher.

Decryption successful (X* = X): True
```

* **Experiment 1: Keystream Entropy Requirements:**
```text
========== Experiment 1: Known plaintext length ==========
TargetBits  ActualBits  Success  gcd(mu,m)    FailureType
       200         200       no          - insufficient-keystream
       250         256       no          - insufficient-keystream
       300         304      yes          1
       350         352      yes          1
```

* **Experiment 2: Modulus Size vs. Computational Latency:**
```text
========== Experiment 2: Modulus size and complexity ==========
Modulus bit length n = 50
Average setup time [us]: 2081.080
Average encryption time [us]: 460.724
Average attack time [us]: 50.449
Successful attacks: 3 / 3

Modulus bit length n = 100
Average setup time [us]: 1112.117
Average encryption time [us]: 567.675
Average attack time [us]: 129.836
Successful attacks: 3 / 3

Modulus bit length n = 128
Average setup time [us]: 4323.006
Average encryption time [us]: 616.844
Average attack time [us]: 170.205
Successful attacks: 3 / 3
```

Experiment 3: Modular Inversion Ambiguity Testing:
```text
========== Experiment 3: Frequency of gcd(mu,m) ≠ 1 ==========
Trials: 20
Cases with gcd(mu, m) ≠ 1: 0
Relative frequency: 0.000
Ambiguous cases reported by the attacker: 0
In ambiguous cases an additional state S4 would be required to uniquely determine the parameters.
```

## 🏗️ Architecture & Context
*High-level system design and execution model.*

* **Objective:** Demonstration of the cryptographic insecurity of LCGs by reconstructing parameters ($A, B, S_0$) from a finite segment of the keystream.
* **Architecture Pattern:** Domain-Driven Design (DDD) with segregation between the Application Layer (Orchestration), Domain Logic (Attack/Cryptography), and Numeric Primitives.
* **Data Flow:**
    1.  **Configuration:** Initialization with Prime Modulus $M$ and random parameters.
    2.  **Encryption:** XOR-based stream encryption via bitstream conversion.
    3.  **Interception:** `LcgKnownPlaintextAttacker` receives partial plaintext/ciphertext pairs.
    4.  **Recovery:** Solving systems of linear congruences to derive multiplier $A$ and increment $B$.
    5.  **Reconstruction:** Back-calculating initial seed $S_0$ for full message decryption.

## ⚖️ Design Decisions & Trade-offs
*Technical justifications for architectural and numerical choices.*

* **Numeric Precision: Arbitrary-Precision Arithmetic**
    * **Context:** Requirement for operations on integers exceeding standard 64-bit capacity (e.g., 128-bit+ primes).
    * **Decision:** Implementation utilizing `BigInteger`.
    * **Rationale:** Provides native support for arbitrary-precision arithmetic without external dependency overhead.
    * **Trade-off:** Prioritized development velocity and mathematical correctness over cache locality and heap-allocation optimization.

* **Security Research: Probabilistic Primality Testing**
    * **Context:** Generating cryptographically relevant moduli requires verifying primality for large integers.
    * **Decision:** Integration of the Miller-Rabin primality test.
    * **Rationale:** Deterministic methods are computationally prohibitive for the required bit lengths in a research context.
    * **Trade-off:** Chose an exponential speed advantage while accepting a cryptographically negligible probability of identifying pseudoprimes.

* **Deployment Strategy: Native AOT Compilation**
    * **Context:** Requirement for high-throughput arithmetic during batched Monte Carlo experiments.
    * **Decision:** Enabling `PublishAot` in the build pipeline.
    * **Rationale:** Minimizes startup latency and memory footprint for the standalone executable.
    * **Trade-off:** Accepted restricted reflection capabilities and longer build times for predictable runtime performance.

## 🧠 Engineering Challenges
*Analysis of non-trivial technical hurdles and implemented solutions.*

* **Ambiguity Management in Modular Inversion:**
    * **Problem:** Solving for multiplier $A$ requires computing the modular inverse of $\mu = S_1 - S_2 \pmod M$; however, an inverse is non-unique if $\gcd(\mu, M) \neq 1$.
    * **Implementation:** `LcgKnownPlaintextAttacker` utilizes the Extended Euclidean Algorithm to detect $\gcd > 1$, flagging the result as `AmbiguousSolutions`.
    * **Outcome:** The system accurately identifies when provided keystream entropy is mathematically insufficient for a unique solution.

* **Bitstream-to-Integer State Alignment:**
    * **Problem:** LCGs operate on full integer states, but stream ciphers consume bits individually, leading to potential misalignment with state boundaries.
    * **Implementation:** `BitConversion` logic partitions the keystream into `stateBitLength` chunks, requiring at least 3 full states ($3n$ bits) before attempting recovery.
    * **Outcome:** Robust handling of arbitrary plaintext fragments, ensuring the information theoretic minimum is met for linear equation solving.

## 🛠️ Tech Stack & Ecosystem
* **Core:** C# 12 / .NET 9.0
* **Numeric Primitives:** `System.Numerics.BigInteger`
* **Infrastructure:** Native AOT (Ahead-of-Time) Compilation
* **Tooling:** Roslyn Performance Analyzers

## 🧪 Quality & Standards
* **Testing Strategy:** `ExperimentRunner` executes Monte Carlo simulations across varying bit lengths (50, 100, 128 bits) with microsecond tracking.
* **Observability:** `AttackResult` objects encapsulate failure domains (Insufficient Keystream, Ambiguity) to allow precise instrumentation.
* **Engineering Principles:** Domain objects utilize immutability for thread-safe experimentation; explicit Boolean array manipulation ensures bitwise correctness.

## 🙋‍♂️ Author

**Kamil Fudala**

- [GitHub](https://github.com/FreakyF)
- [LinkedIn](https://www.linkedin.com/in/kamil-fudala/)

## ⚖️ License

This project is licensed under the [MIT License](LICENSE).
