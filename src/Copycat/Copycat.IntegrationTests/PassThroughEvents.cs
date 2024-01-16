namespace Copycat.IntegrationTests;

public interface IWithEvent
{
    event EventHandler<EventArgs> SomeEvent;
}

[Decorate]
public partial class PassThroughEvents : IWithEvent { }