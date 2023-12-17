using System.Diagnostics;

namespace Copycat.IntegrationTests;

public interface ISomeInterface
{
    void DoNothing();
    void DoSomething();
    void DoSomethingElse(int a, string b);
}

[Decorate]
public partial class SimpleDecorator : ISomeInterface
{
    private readonly ISomeInterface _decorated;

    public SimpleDecorator(ISomeInterface decorated) => 
        _decorated = decorated;

    [Template]
    public void CalculateElapsedTime(Action action)
    {
        var sw = Stopwatch.StartNew();
        action();
        Console.WriteLine($"{nameof(action)} took {sw.ElapsedMilliseconds} ms");
    }
    
    public void DoNothing() { }
}
