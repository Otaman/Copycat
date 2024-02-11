namespace Copycat.IntegrationTests;

[Decorate]
public partial class DecorateClosedAndRenamedTypeParameters<TValue> : IGenericWithTwoTypeParameters<User, TValue>
{
    [Template]
    private void LogValue(Action action)
    {
        action();
        Console.WriteLine($"Called {nameof(action)}");
    }
}