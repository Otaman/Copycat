namespace Copycat.IntegrationTests;

public interface IWithProperties
{
    string Name { get; set; }
    int Age { get; set; }
    
    bool GetOnly { get; }
    object SetOnly { set; }
    
    string SayHello();
}

[Decorate]
public partial class PassThroughProperties : IWithProperties { }