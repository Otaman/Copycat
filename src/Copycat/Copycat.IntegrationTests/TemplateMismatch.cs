namespace Copycat.IntegrationTests;

public interface ITemplateMismatch
{
    void DoSomething();
    int ReturnSomething();
}

#pragma warning disable CC1001

// Should emit build warning for mismatched template
[Decorate]
public partial class TemplateMismatch : ITemplateMismatch
{
    [Template]
    private void DoubleDo(Action action)
    {
        action();
        action();
    }
}