namespace Lab06.Domain.Cryptanalysis;

/// <summary>
/// Represents the result of a cryptanalytic attack, containing the recovered internal states.
/// </summary>
/// <param name="StateX">The recovered initial state of the first LFSR (X).</param>
/// <param name="StateY">The recovered initial state of the second LFSR (Y).</param>
/// <param name="StateZ">The recovered initial state of the third LFSR (Z).</param>
public record struct AttackResult(int[] StateX, int[] StateY, int[] StateZ);
