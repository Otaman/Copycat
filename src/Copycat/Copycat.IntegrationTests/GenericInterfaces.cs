namespace Copycat.IntegrationTests;

public interface IGenericWithSingleTypeParameter<T>
{
    T Value { get; set; }
    T? TryGetValue();
    void SetValue(T value);
}

public interface IGenericWithTwoTypeParameters<T1, T2>
{
    T1 Value1 { get; set; }
    T2 Value2 { get; set; }
    T1? TryGetValue1();
    T2? TryGetValue2();
    void SetValue1(T1 value);
    void SetValue2(T2 value);
}

