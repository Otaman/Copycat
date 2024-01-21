namespace Copycat.IntegrationTests;

public interface ICommonInterface
{
    string Name { get; set; }
    event EventHandler SomethingHappened;
    string this[int index] { get; set; }
    void DoCommonThing();
}

public interface IDoOneThing : ICommonInterface
{
    string OneThing { get; set; }
    event EventHandler OneThingHappened;
    string this[bool index] { get; set; }
    void DoSomething();
}

public interface IDoAnotherThing : ICommonInterface
{
    string AnotherThing { get; set; }
    event EventHandler AnotherThingHappened;
    string this[double index] { get; set; }
    int ReturnSomething();
}

public interface IDescendantInterface : IDoOneThing, IDoAnotherThing
{
    string DescendantThing { get; set; }
    event EventHandler DescendantThingHappened;
    string this[long index] { get; set; }
    void DoSomethingElse();
}

[Decorate]
public partial class InterfaceHierarchy : IDescendantInterface
{
    
}