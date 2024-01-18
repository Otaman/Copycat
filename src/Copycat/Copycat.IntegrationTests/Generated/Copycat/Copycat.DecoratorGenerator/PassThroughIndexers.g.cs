﻿// <auto-generated/>
namespace Copycat.IntegrationTests;
public partial class PassThroughIndexers
{
    private Copycat.IntegrationTests.IWithIndexers _decorated;
    public PassThroughIndexers(Copycat.IntegrationTests.IWithIndexers decorated)
    {
        _decorated = decorated;
    }

    public string this[int index] { get => _decorated[index]; set => _decorated[index] = value; }

    public string this[string key, string suffix] { get => _decorated[key, suffix]; set => _decorated[key, suffix] = value; }

    public int this[string key] { get => _decorated[key]; }

    public int this[bool key] { set => _decorated[key] = value; }
}