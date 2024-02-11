# Copycat
Copycat is a source generator for creating decorators by templates.
The source generator intents to simplify implementation of a [Decorator Pattern](https://en.wikipedia.org/wiki/Decorator_pattern).

## Basic Use Case
To activate generator, use DecorateAttribute on a class. The class must be partial and have exactly one interface to decorate:
```C#
using Copycat;

public interface ISomeInterface
{
    void DoSomething();
    void DoSomethingElse(int a, string b);
}

[Decorate]
public partial class SimpleDecorator : ISomeInterface { }
```

In this example, Copycat generates pass-through decorator:
```C#
// <auto-generated/>
public partial class SimpleDecorator
{
    private ISomeInterface _decorated;
    public SimpleDecorator(ISomeInterface decorated)
    {
        _decorated = decorated;
    }

    public void DoSomething() => _decorated.DoSomething();

    public void DoSomethingElse(int a, string b) => _decorated.DoSomethingElse(a, b);
}
```

To decorate with custom logic, use TemplateAttribute:
```C#
public interface ICache<T>
{
    Task<T> Get(string key);
    Task<T> Set(string key, T value);
}

[Decorate]
public partial class CacheDecorator<T> : ICache<T>
{
    [Template]
    public async Task<T> RetryOnce(Func<Task<T>> action, string key)
    {
        try
        {
            return await action();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Retry {nameof(action)} for {key} due to {e.Message}");
            return await action();
        }
    }
}
```
Template is a method that takes parameterless delegate which has the same return type as the method itself.
Use any names for the template method and a delegate (as usual, it's better to keep them self-explanatory).

Copycat then generates decorator based on the template:
```C#
// <auto-generated/>
public partial class CacheDecorator<T>
{
    private ICache<T> _decorated;
    public CacheDecorator(ICache<T> decorated)
    {
        _decorated = decorated;
    }

    /// <see cref = "CacheDecorator.RetryOnce(Func{Task{T}}, string)"/>
    public async Task<T> Get(string key)
    {
        try
        {
            return await _decorated.Get(key);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Retry {nameof(Get)} for {key} due to {e.Message}");
            return await _decorated.Get(key);
        }
    }

    /// <see cref = "CacheDecorator.RetryOnce(Func{Task{T}}, string)"/>
    public async Task<T> Set(string key, T value)
    {
        try
        {
            return await _decorated.Set(key, value);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Retry {nameof(Set)} for {key} due to {e.Message}");
            return await _decorated.Set(key, value);
        }
    }
}
```

## Advanced Use Cases
That was only basic usage, see repository [Wiki](https://github.com/Otaman/Copycat) for more examples and details.