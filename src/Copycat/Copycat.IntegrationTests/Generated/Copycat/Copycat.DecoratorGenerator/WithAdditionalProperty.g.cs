﻿// <auto-generated/>
namespace Copycat.IntegrationTests;
public partial class WithAdditionalProperty
{
    private Copycat.IntegrationTests.IEmpty _decorated;
    public WithAdditionalProperty(Copycat.IntegrationTests.IEmpty decorated, Copycat.IntegrationTests.IDependency dependency)
    {
        _decorated = decorated;
        Dependency = dependency;
    }
}