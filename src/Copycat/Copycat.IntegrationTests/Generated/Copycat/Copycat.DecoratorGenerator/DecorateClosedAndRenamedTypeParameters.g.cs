﻿// <auto-generated/>
namespace Copycat.IntegrationTests;
public partial class DecorateClosedAndRenamedTypeParameters<TValue>
{
    private Copycat.IntegrationTests.IGenericWithTwoTypeParameters<Copycat.IntegrationTests.User, TValue> _decorated;
    public DecorateClosedAndRenamedTypeParameters(Copycat.IntegrationTests.IGenericWithTwoTypeParameters<Copycat.IntegrationTests.User, TValue> decorated)
    {
        _decorated = decorated;
    }

    public Copycat.IntegrationTests.User Value1 { get => _decorated.Value1; set => _decorated.Value1 = value; }

    public TValue Value2 { get => _decorated.Value2; set => _decorated.Value2 = value; }

    public Copycat.IntegrationTests.User? TryGetValue1() => _decorated.TryGetValue1();
    public TValue? TryGetValue2() => _decorated.TryGetValue2();
    /// <see cref = "DecorateClosedAndRenamedTypeParameters.LogValue(Action)"/>
    public void SetValue1(Copycat.IntegrationTests.User value)
    {
        _decorated.SetValue1(value);
        Console.WriteLine($"Called {nameof(SetValue1)}");
    }

    /// <see cref = "DecorateClosedAndRenamedTypeParameters.LogValue(Action)"/>
    public void SetValue2(TValue value)
    {
        _decorated.SetValue2(value);
        Console.WriteLine($"Called {nameof(SetValue2)}");
    }
}