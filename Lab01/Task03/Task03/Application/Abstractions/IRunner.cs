using Task03.Application.Models;

namespace Task03.Application.Abstractions;

public interface IRunner
{
    /// <summary>Executes the cryptographic workflow using the provided options and returns the process exit code.</summary>
    /// <param name="options">The validated application options describing paths and operation mode.</param>
    /// <returns>An integer exit code where zero indicates success and non-zero indicates failure.</returns>
    int Run(AppOptions options);
}