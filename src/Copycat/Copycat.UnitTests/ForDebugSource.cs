namespace Copycat.UnitTests;

public interface INotEmpty
{
    int Id { get; }
    void Do();
    event EventHandler Event;
    int this[int index] { get; }
}
public interface IDependency { }

[Decorate]
public partial class WithAdditionalProperty : INotEmpty
{
    private IDependency Dependency { get; }
}