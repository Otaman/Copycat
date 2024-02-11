namespace Copycat.IntegrationTests;

public interface IWithRefOut
{
    void DoSomething(ref int value);
    void DoSomethingElse(out int value);
}

[Decorate]
public partial class PassThroughRefOut : IWithRefOut { }