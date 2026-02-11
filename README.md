# Cryptography and Cryptanalysis | High-Performance Experimental Laboratories

A technical research platform dedicated to the exploration of cryptographic primitives and the implementation of advanced cryptanalytic attacks. This repository serves as an engineering workbench for testing high-throughput stream ciphers, stochastic key recovery, and algebraic state reconstruction.

## 🚀 Laboratories Overview

**[Laboratory 01: Substitution Cryptanalysis](Lab01-Substitution-Analysis)**
Modular statistical environment for substitution ciphers utilizing n-gram frequency distributions and Chi-Square divergence testing to quantify linguistic similarity.

**[Laboratory 02: Classical Cipher Suite](Lab02-Classical-Ciphers)**
Automated brute-force engine for Caesar and Affine ciphers implementing the Extended Euclidean Algorithm and result-pattern orchestration for explicit error handling.

**[Laboratory 03: Heuristic Key Recovery](Lab03-Heuristic-Cryptanalysis)**
Stochastic cryptanalysis workbench utilizing Metropolis-Hastings and Simulated Annealing with `Unsafe` hot-path optimizations—**achieving a 99.5% reduction in execution time (from 100s to 519ms)**.

**[Laboratory 04: LCG State Reconstruction](Lab04-LCG-Cryptanalysis)**
Known-plaintext attack implementation targeting Linear Congruential Generators, leveraging arbitrary-precision arithmetic to solve systems of linear congruences.

**[Laboratory 05: LFSR & GF(2) Solvers](Lab05-LFSR-Stream-Cipher)**
Stream cipher analysis framework featuring a zero-allocation Gaussian elimination solver over $GF(2)$ optimized with 64-bit word-packing and `stackalloc` buffers.

**[Laboratory 06: Geffe Correlation Attack](Lab06-Geffe-Generator-Attack)**
Divide-and-conquer statistical attack suite targeting non-linear Geffe combiners through Pearson Rho correlation coefficients to recover internal states in sub-linear time.

**[Laboratory 07: Trivium SIMD & Cube Attack](Lab07-Trivium-Cube-Attack)**
High-throughput Trivium implementation leveraging AVX2 SIMD parallelization (8-stream) and algebraic Cube Attacks—**reaching peak throughput of 70 Gbps**.

## 🏗️ Technical Highlights

* **High-Performance Compute:** Extensive use of `Span<T>`, `stackalloc`, and `Unsafe` pointer arithmetic to achieve zero-allocation in cryptographic hot paths.
* **Vectorization:** Implementation of AVX2 intrinsics to parallelize stream cipher state updates, processing 8 independent streams in 256-bit registers.

* **Advanced Cryptanalysis:** Application of stochastic (MCMC) and algebraic (Cube Attack) methods to break ciphers without exhaustive key searches.
* **Compilation Strategy:** Systematic use of Native AOT (`PublishAot`) to ensure predictable execution latency and minimal memory footprint.

## 🛠️ Tech Stack & Runtime Environment

* **Languages:** C# 13, C# 14
* **Runtimes:** .NET 9.0, .NET 10.0
* **Numerical Core:** `System.Numerics.BigInteger`, Custom `Xoshiro256` PRNG
* **Low-Level Tools:** AVX2 SIMD Intrinsics, `System.Runtime.CompilerServices.Unsafe`, and `stackalloc`
* **Deployment:** Native AOT for self-contained, high-performance binaries

## 🧪 Quality & Standards

* **Validation:** All implementations verified against Known Answer Tests (KAT) or through Monte Carlo simulations to ensure statistical reliability.
* **Optimization Principles:** Focus on cache locality, branchless programming, and zero-allocation in execution "hot loops".
* **Architecture:** Adherence to Vertical Slice and Layered DDD-Lite principles to maintain modularity in performance-critical kernels.

## 🙋‍♂️ Author

**Kamil Fudala**

- [GitHub](https://github.com/FreakyF)
- [LinkedIn](https://www.linkedin.com/in/kamil-fudala/)

## ⚖️ License

This project is licensed under the [MIT License](LICENSE).
