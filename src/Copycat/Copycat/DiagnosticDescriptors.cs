using Microsoft.CodeAnalysis;

namespace Copycat;

// Release tracking analyzer does not generate valid markdown, so disable it
#pragma warning disable RS2008

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor FailedToGenerateDecorator = new (
        id: "CC0001",
        title: "Copycat failed to generate decorator",
        messageFormat: "Copycat failed to generate decorator for class {0} due to {1}: {2}",
        category: "Copycat.DecoratorGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static readonly DiagnosticDescriptor NoMatchingTemplateForMethod = new (
        id: "CC1001",
        title: "Copycat found template(s) but none matched",
        messageFormat: "Copycat found template(s) but none matched the {0} method of interface {1} in class {2}. " +
                       "Generated default pass-through implementation.",
        category: "Copycat.TemplateSelector",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
    
    public static readonly DiagnosticDescriptor TemplateSourceCodeIsNotAccessible = new (
        id: "CC1002",
        title: "Copycat cannot access template source code",
        messageFormat: "Copycat ignored template {0} that is not defined inside of the current project. " +
                       "Template source code is not accessible and cannot be used to generate decorator.",
        category: "Copycat.TemplateSelector",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}