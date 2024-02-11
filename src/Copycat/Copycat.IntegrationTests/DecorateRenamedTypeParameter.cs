namespace Copycat.IntegrationTests;

[Decorate]
public partial class DecorateRenamedTypeParameter<TRenamed> : IGenericWithSingleTypeParameter<TRenamed>
{
    [Template]
    private void LogValue(Action action, TRenamed value)
    {
        action();
        Console.WriteLine($"Value: {value}");
    }
}