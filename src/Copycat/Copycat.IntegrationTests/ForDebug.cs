using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Copycat.IntegrationTests;

[TestFixture, Explicit]
public class ForDebug
{
    [Test]
    public void ShouldResolveTemplate()
    {
        var source = @"
using Copycat;

namespace Copycat.IntegrationTests;

public interface IReporter
{
    Task Report(string message);
    Task Report(string message, Exception exception);
}

[Decorate]
public partial class TaskWrapper : IReporter
{
    private readonly IReporter _reporter;

    public TaskWrapper(IReporter reporter) => _reporter = reporter;

    [Template]
    public async Task WrapTask(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception e)
        {
            await _reporter.Report(""Failed to execute action"", e);
        }
    }
}";

        var r = TestHelpers.GetGeneratedOutput<DecoratorGenerator>(source);
    }
}

internal class TestHelpers
{
    public static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput<T>(string source)
        where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
            .Select(_ => MetadataReference.CreateFromFile(_.Location))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DecorateAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute).Assembly.Location),
            });

        var compilation = CSharpCompilation.Create(
            "generator",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var trees = outputCompilation.SyntaxTrees.ToList();

        return (diagnostics, trees.Count != originalTreeCount ? trees[^1].ToString() : string.Empty);
    }
}