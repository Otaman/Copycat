using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Copycat;

internal class SymbolFinder
{
    private readonly INamedTypeSymbol _source;
    private readonly SemanticModel _semantic;

    public SymbolFinder(INamedTypeSymbol source, SemanticModel semantic)
    {
        _source = source;
        _semantic = semantic;
    }

    public ImmutableArray<IMethodSymbol> FindTemplates() =>
        TraverseMethods(_source, x => x.GetAttributes().Any(IsTemplateAttribute)).ToImmutableArray();

    public ImmutableArray<IMethodSymbol> FindNotImplementedMethods(INamedTypeSymbol sourceInterface) =>
        GetAllInterfaceMembers(sourceInterface).OfType<IMethodSymbol>()
            .Where(x => x.MethodKind == MethodKind.Ordinary)
            .Where(x => _source.FindImplementationForInterfaceMember(x) == null)
            .ToImmutableArray();
    
    public ImmutableArray<IPropertySymbol> FindNotImplementedProperties(INamedTypeSymbol sourceInterface) =>
        GetAllInterfaceMembers(sourceInterface).OfType<IPropertySymbol>()
            .Where(x => !x.IsIndexer)
            .Where(x => _source.FindImplementationForInterfaceMember(x) == null)
            .ToImmutableArray();
    
    public ImmutableArray<IEventSymbol> FindNotImplementedEvents(INamedTypeSymbol sourceInterface) =>
        GetAllInterfaceMembers(sourceInterface).OfType<IEventSymbol>()
            .Where(x => _source.FindImplementationForInterfaceMember(x) == null)
            .ToImmutableArray();
    
    public ImmutableArray<IPropertySymbol> FindNotImplementedIndexers(INamedTypeSymbol sourceInterface) =>
        GetAllInterfaceMembers(sourceInterface).OfType<IPropertySymbol>()
            .Where(x => x.IsIndexer)
            .Where(x => _source.FindImplementationForInterfaceMember(x) == null)
            .ToImmutableArray();
        
    public ImmutableArray<ISymbol> FindFieldsOrPropertiesOfType(INamedTypeSymbol typeToSearch) =>
        _source.GetMembers()
            .Where(x => !x.IsStatic)
            .Where(x => !x.IsImplicitlyDeclared)
            .Where(x => x is IFieldSymbol field && SymbolEqualityComparer.Default.Equals(field.Type, typeToSearch) ||
                        x is IPropertySymbol property && SymbolEqualityComparer.Default.Equals(property.Type, typeToSearch))
            .ToImmutableArray();

    public IEnumerable<(string type, string name)> FindReadonlyUninitializedFieldsOrProperties() =>
        FindReadonlyUninitializedFields().Select(x => (x.Type.ToDisplayString(), x.Name))
            .Concat(FindReadonlyUninitializedProperties().Select(x => (x.Type.ToDisplayString(), x.Name)));

    private IEnumerable<IFieldSymbol> FindReadonlyUninitializedFields() =>
        _source.GetMembers()
            .Where(x => x is IFieldSymbol { IsReadOnly: true, IsImplicitlyDeclared: false } field && !IsInitialized(field))
            .Cast<IFieldSymbol>();
    
    private IEnumerable<IPropertySymbol> FindReadonlyUninitializedProperties() =>
        _source.GetMembers()
            .Where(x => x is IPropertySymbol { IsReadOnly: true, IsImplicitlyDeclared: false } property && !IsInitialized(property))
            .Cast<IPropertySymbol>();
    
    private bool IsInitialized(IFieldSymbol symbol)
    {
        var syntax = symbol.DeclaringSyntaxReferences.Single().GetSyntax();

        // initialized in declaration
        if (syntax is VariableDeclaratorSyntax { Initializer: not null })
            return true;

        return FindAllAssignmentsInConstructors().Contains(symbol, SymbolEqualityComparer.Default);
    }
    
    private bool IsInitialized(IPropertySymbol symbol)
    {
        var syntax = symbol.DeclaringSyntaxReferences.Single().GetSyntax();

        // initialized in declaration
        if (syntax is PropertyDeclarationSyntax { Initializer: not null })
            return true;

        return FindAllAssignmentsInConstructors().Contains(symbol, SymbolEqualityComparer.Default);
    }
    
    private ImmutableArray<ISymbol> FindAllAssignmentsInConstructors() =>
        FindConstructors()
            .SelectMany(x => x.DeclaringSyntaxReferences)
            .Select(x => x.GetSyntax())
            .OfType<ConstructorDeclarationSyntax>()
            .SelectMany(x => x.DescendantNodes().OfType<AssignmentExpressionSyntax>())
            .Select(x => _semantic.GetSymbolInfo(x.Left).Symbol)
            .Where(x => x is IFieldSymbol or IPropertySymbol)
            .Cast<ISymbol>()
            .ToImmutableArray();
        
    // find all constructors
    public ImmutableArray<IMethodSymbol> FindConstructors() =>
        _source.Constructors
            .Where(x => x.MethodKind == MethodKind.Constructor)
            .Where(x => !x.IsStatic)
            .Where(x => !x.IsImplicitlyDeclared)
            .ToImmutableArray();

    public ImmutableArray<IMethodSymbol> FindBaseConstructors()
    {
        // check for null and not object
        if(_source.BaseType == null || _source.BaseType.SpecialType == SpecialType.System_Object)
            return ImmutableArray<IMethodSymbol>.Empty;
        
        return _source.BaseType.Constructors
            .Where(x => x.MethodKind == MethodKind.Constructor)
            .Where(x => !x.IsStatic)
            .Where(x => !x.IsImplicitlyDeclared)
            .ToImmutableArray();
    }
    
    private static IEnumerable<IMethodSymbol> TraverseMethods(INamedTypeSymbol source, Func<IMethodSymbol, bool> filter)
    {
        var result = source.GetMembers().OfType<IMethodSymbol>().Where(filter);
        if (source.BaseType != null)
            result = result.Concat(TraverseMethods(source.BaseType, filter));
        
        return result;
    }

    private static bool IsTemplateAttribute(AttributeData y) => 
        y.AttributeClass is { Name: nameof(TemplateAttribute) };
    
    private ImmutableArray<ISymbol> GetAllInterfaceMembers(INamedTypeSymbol interfaceSymbol) =>
        GetAllInterfaceMembersImpl(interfaceSymbol, new HashSet<string>()).ToImmutableArray();
        
    
    private IEnumerable<ISymbol> GetAllInterfaceMembersImpl(INamedTypeSymbol interfaceSymbol, HashSet<string> visitedMembers)
    {
        foreach (var member in interfaceSymbol.GetMembers())
        {
            var displayString = member.ToDisplayString(SymbolDisplayFormats.DetailedNoReturnType);
            if(visitedMembers.Add(displayString))
                yield return member;
        }

        foreach (var parentInterface in interfaceSymbol.Interfaces)
        {
            foreach (var member in GetAllInterfaceMembersImpl(parentInterface, visitedMembers))
            {
                if(interfaceSymbol.FindImplementationForInterfaceMember(member) == null)
                    yield return member;
            }
        }
    }
}