namespace Copycat.IntegrationTests;

public interface IPassThrough
{
    void DoSomething();
    int ReturnSomething();
}

[Decorate]
public partial class PassThrough : IPassThrough { }