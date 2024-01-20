using System.Diagnostics;
using Serilog;

namespace Copycat.IntegrationTests;

public class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int Version { get; set; }
}

public class UserException : Exception
{
    public Guid UserId { get; }
    
    public UserException(Guid userId, string message) : base(message)
    {
        UserId = userId;
    }
}

public interface IUserService
{
    User GetUser(Guid id);
    User CreateUser(Guid id, string name);
    User UpdateUserName(Guid id, string name);
    void DeleteUser(Guid id);
}

[Decorate]
public partial class ExceptionWrapper : IUserService
{
    private readonly IUserService _decorated;
    private readonly ILogger _logger;

    public ExceptionWrapper(IUserService decorated, ILogger logger)
    {
        _decorated = decorated;
        _logger = logger.ForContext<ExceptionWrapper>();
    }

    [Template]
    public User WrapExceptions(Func<User> action, Guid id)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var user = action();
            _logger.Information("{Action} for user {Id} took {ElapsedMs} ms, current version is {Version}", 
                nameof(action), id, sw.ElapsedMilliseconds, user.Version);
            return user;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to execute {Action} for user {Id}, took {ElapsedMs} ms",
                nameof(action), id, sw.ElapsedMilliseconds);
            throw new UserException(id, e.Message);
        }
    }
    
    public void DeleteUser(Guid id) => _decorated.DeleteUser(id);
}