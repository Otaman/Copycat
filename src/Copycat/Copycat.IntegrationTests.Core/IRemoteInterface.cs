namespace Copycat.IntegrationTests.Core;

public interface IRemoteInterface
{
    int Property { get; set; }
    
    event EventHandler<EventArgs> Event;
    
    string this[int index] { get; set; }
    
    string Method(string arg);
    
    void MethodVoid();
    
    Task MethodTask();
    
    Task<string> MethodTaskString();
}