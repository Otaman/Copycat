namespace Copycat.IntegrationTests;

public interface IReportDiagnostic
{
    void Report(string message);
}

#pragma warning disable CC0001

// Should fail to create decorator, but should not fail to generate other decorators
[Decorate]
public partial class DecoratorGeneratorDiagnostics : IReportDiagnostic
{
    // only one field or property is allowed of type IReportDiagnostic to generate decorator
    private readonly IReportDiagnostic _decorated;
    private IReportDiagnostic Decorated { get; }

    public DecoratorGeneratorDiagnostics(IReportDiagnostic decorated)
    {
        _decorated = decorated;
        Decorated = decorated;
    }

    public void Report(string message) =>
        _decorated.Report(message);
}