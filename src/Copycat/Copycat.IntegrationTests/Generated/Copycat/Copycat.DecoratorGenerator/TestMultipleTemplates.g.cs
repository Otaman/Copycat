﻿// <auto-generated/>
namespace Copycat.IntegrationTests;
public partial class TestMultipleTemplates
{
    /// <see cref = "TestMultipleTemplates.Template1(Action)"/>
    public void DoSomething()
    {
        Console.WriteLine("Template1");
        _decorated.DoSomething();
    }

    /// <see cref = "TestMultipleTemplates.Template2(Func{int})"/>
    public int ReturnSomething()
    {
        Console.WriteLine("Template2");
        return _decorated.ReturnSomething();
    }
}