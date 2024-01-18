namespace Copycat.UnitTests;

public interface IEmpty { }
public interface IDependency { }

[Decorate]
public partial class WithAdditionalProperty : IEmpty
{
    private IDependency Dependency { get; }
}