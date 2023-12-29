namespace Copycat.IntegrationTests;

public interface ITestMultipleTemplates
{
    void DoSomething();
    int ReturnSomething();
}

[Decorate]
public partial class TestMultipleTemplates : ITestMultipleTemplates
{
    private readonly ITestMultipleTemplates _decorated;
    
    public TestMultipleTemplates(ITestMultipleTemplates decorated) => 
        _decorated = decorated;
    
    [Template]
    private void Template1(Action action)
    {
        Console.WriteLine("Template1");
        action();
    }
    
    [Template]
    private int Template2(Func<int> action)
    {
        Console.WriteLine("Template2");
        return action();
    }
}