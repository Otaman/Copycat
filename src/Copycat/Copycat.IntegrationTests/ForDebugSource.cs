namespace Copycat.IntegrationTests;

// Should create constructor that takes IEmpty and assigns it to field

[Decorate]
public partial class WithDecoratedProperty : IEmpty
{
    private IEmpty Decorated { get; }
}