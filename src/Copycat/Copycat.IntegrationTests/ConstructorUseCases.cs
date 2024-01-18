namespace Copycat.IntegrationTests;

public interface IEmpty { }

[Decorate]
public partial class CompletelyEmpty : IEmpty { }

// Should create constructor that takes IEmpty and assigns it to field

[Decorate]
public partial class WithDecoratedField : IEmpty
{
    private readonly IEmpty _decorated;
}

public class Empty : IEmpty { }

// Should not create constructor

[Decorate]
public partial class WithInitializedDecoratedField : IEmpty
{
    private readonly IEmpty _decorated = new Empty();
}

public interface IDependency { }

// Should create _decorated field, constructor that takes IEmpty and IDependency and assigns them
[Decorate]
public partial class WithAdditionalField : IEmpty
{
    private readonly IDependency _dependency;
}

// Should create _decorated field, constructor that takes IEmpty and assigns it and calls other constructor
[Decorate]
public partial class WithAdditionalFieldAndConstructor : IEmpty
{
    private readonly IDependency _dependency;

    public WithAdditionalFieldAndConstructor(IDependency dependency) =>
        _dependency = dependency;
}

public class Dependency : IDependency { }

// Should create _decorated field, constructor that takes IEmpty and assigns it and calls other constructor
[Decorate]
public partial class WithAdditionalFieldAndParameterlessConstructor : IEmpty
{
    private readonly IDependency _dependency;

    public WithAdditionalFieldAndParameterlessConstructor() =>
        _dependency = new Dependency();
}

// Should create constructor that takes IEmpty and assigns it to field

[Decorate]
public partial class WithDecoratedProperty : IEmpty
{
    private IEmpty Decorated { get; }
}

// Should create _decorated field, constructor that takes IEmpty and IDependency and assigns them
[Decorate]
public partial class WithAdditionalProperty : IEmpty
{
    private IDependency Dependency { get; }
}

// Should create _decorated field, constructor that takes IEmpty and assigns it and calls other constructor
[Decorate]
public partial class WithAdditionalPropertyAndParameterlessConstructor : IEmpty
{
    private IDependency Dependency { get; }

    public WithAdditionalPropertyAndParameterlessConstructor() =>
        Dependency = new Dependency();
}


public abstract class BaseClass
{
    protected readonly IDependency Dependency;
    
    protected BaseClass(IDependency dependency) =>
        Dependency = dependency;
}

// Should use base class constructor
[Decorate]
public partial class EmptyWithBaseClass : BaseClass, IEmpty
{
}