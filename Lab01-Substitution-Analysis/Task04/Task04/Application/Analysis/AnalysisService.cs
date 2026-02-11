using Task04.Application.Abstractions;

namespace Task04.Application.Analysis;

public sealed class AnalysisServices(
    INGramCounter nGramCounter,
    IReferenceLoader referenceLoader,
    IChiSquareCalculator chiSquare)
{
    public INGramCounter NGramCounter { get; } = nGramCounter ?? throw new ArgumentNullException(nameof(nGramCounter));

    public IReferenceLoader ReferenceLoader { get; } =
        referenceLoader ?? throw new ArgumentNullException(nameof(referenceLoader));

    public IChiSquareCalculator ChiSquare { get; } = chiSquare ?? throw new ArgumentNullException(nameof(chiSquare));
}