using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Copycat;

internal class SymbolFinder
{
    private readonly INamedTypeSymbol _source;

    public SymbolFinder(INamedTypeSymbol source) => _source = source;

    public ImmutableArray<IMethodSymbol> FindTemplates() =>
        _source.GetMembers().OfType<IMethodSymbol>()
            .Where(x => x.GetAttributes().Any(IsTemplateAttribute))
            .ToImmutableArray();

    public ImmutableArray<IMethodSymbol> FindNotImplementedMethods(INamedTypeSymbol sourceInterface) =>
        sourceInterface.GetMembers().OfType<IMethodSymbol>()
            .Where(x => _source.FindImplementationForInterfaceMember(x) == null)
            .ToImmutableArray();
        
    public ImmutableArray<ISymbol> FindFieldsOrPropertiesOfType(INamedTypeSymbol typeToSearch) =>
        _source.GetMembers()
            .Where(x => x is IFieldSymbol field && field.Type.Equals(typeToSearch) ||
                        x is IPropertySymbol property && property.Type.Equals(typeToSearch))
            .ToImmutableArray();
        
    // find all constructors
    public ImmutableArray<IMethodSymbol> FindConstructors() =>
        _source.GetMembers().OfType<IMethodSymbol>()
            .Where(x => x.MethodKind == MethodKind.Constructor)
            .Where(x => !x.IsStatic)
            .Where(x => !x.IsImplicitlyDeclared)
            .ToImmutableArray();
        
    private static bool IsTemplateAttribute(AttributeData y) => 
        y.AttributeClass is { Name: nameof(TemplateAttribute) };
}