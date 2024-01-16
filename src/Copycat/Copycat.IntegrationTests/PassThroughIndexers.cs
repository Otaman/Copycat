namespace Copycat.IntegrationTests;

public interface IWithIndexers
{
    string this[int index] { get; set; }
    string this[string key, string suffix] { get; set; }
    
    int this[string key] { get; }
    int this[bool key] { set; }
}

[Decorate]
public partial class PassThroughIndexers : IWithIndexers { }