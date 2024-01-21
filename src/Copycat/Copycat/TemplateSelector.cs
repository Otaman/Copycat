using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Copycat;

internal class TemplateSelector
{
    private readonly ImmutableArray<IMethodSymbol> _templates;

    public TemplateSelector(ImmutableArray<IMethodSymbol> templates) => 
        _templates = templates;

    public IMethodSymbol? FindTemplateForMethod(IMethodSymbol method)
    {
        var candidates = _templates.Where(x => IsCandidate(x, method)).ToImmutableArray();
        if (candidates.IsEmpty)
            return null;
        
        var selected = candidates.OrderByDescending(x => x, new TemplateCandidatesComparer(method)).First();
        return selected;
    }
    
    // naive implementation
    private static bool IsCandidate(IMethodSymbol template, IMethodSymbol method)
    {
        // has compatible return type
        if(method.ReturnsVoid && !template.ReturnsVoid || !template.IsGenericMethod && !SymbolEqualityComparer.Default.Equals(template.ReturnType, method.ReturnType))
            return false;
        
        if (template is { IsGenericMethod: true, TypeParameters.Length: 1 } && !SymbolEqualityComparer.Default.Equals(template.ReturnType, method.ReturnType))
        {
            var typeParameter = template.TypeParameters[0];
            
            // check if type parameter has constraints
            if (typeParameter.HasConstructorConstraint || 
                typeParameter.HasReferenceTypeConstraint || 
                typeParameter.HasValueTypeConstraint || 
                typeParameter.HasNotNullConstraint ||
                typeParameter.HasUnmanagedTypeConstraint ||
                !typeParameter.ConstraintTypes.IsEmpty)
                return false;
            
            // check if type parameter is returned directly. Example: T Template<T>(...){} where T is returned directly
            if (SymbolEqualityComparer.Default.Equals(template.ReturnType, typeParameter))
            {
                // and method return type is not Task or ValueTask or Task<Something> or ValueTask<Something>
                return !ReturnsTask(method);
            }
            
            // check if type parameter is returned as awaitable. Example: Task<T> Template<T>(...){} where T is returned as Task<T>
            if (IsGenericAndReturnsTask(template))
            {
                // and method return type is Task or ValueTask or Task<Something> or ValueTask<Something>
                return ReturnsTask(method);
            }
        }
        
        // has compatible parameters
        // skip first parameter, because it is the action
        for (var i = 1; i < template.Parameters.Length; i++)
        {
            var templateParameter = template.Parameters[i];
            var methodParameter = method.Parameters.FirstOrDefault(x => x.Name == templateParameter.Name);
            if (methodParameter == null)
                return false;

            if (!SymbolEqualityComparer.Default.Equals(templateParameter.Type, methodParameter.Type))
                return false;
        }

        return true;
    }

    private static bool ReturnsTask(IMethodSymbol method)
    {
        return method.ReturnType is INamedTypeSymbol namedType &&
               (namedType.Name == "Task" || namedType.Name == "ValueTask");
    }
    
    private static bool IsGenericAndReturnsTask(IMethodSymbol method)
    {
        return method.ReturnType is INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } namedType &&
               (namedType.Name == "Task" || namedType.Name == "ValueTask") &&
               SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], method.TypeParameters[0]);
    }

    private class TemplateCandidatesComparer : Comparer<IMethodSymbol>
    {
        private readonly IMethodSymbol _method;
    
        public TemplateCandidatesComparer(IMethodSymbol method) => 
            _method = method;
    
        public override int Compare(IMethodSymbol? x, IMethodSymbol? y)
        {
            // null check
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            
            // prefer with same return type
            if (SymbolEqualityComparer.Default.Equals(x.ReturnType, _method.ReturnType) && 
                !SymbolEqualityComparer.Default.Equals(y.ReturnType, _method.ReturnType)) return 1;
            if (!SymbolEqualityComparer.Default.Equals(x.ReturnType, _method.ReturnType) && 
                SymbolEqualityComparer.Default.Equals(y.ReturnType, _method.ReturnType)) return -1;
            
            var genericTypeComparison = x.IsGenericMethod.CompareTo(y.IsGenericMethod);
            if (genericTypeComparison != 0)
                return -genericTypeComparison; // prefer non-generic methods

            if (x.IsGenericMethod && y.IsGenericMethod)
            {
                var awaitableComparison = IsGenericAndReturnsTask(x).CompareTo(IsGenericAndReturnsTask(y));
                if (awaitableComparison != 0)
                    return awaitableComparison; // prefer awaitable methods
            }
        
            // the more parameters the better
            var parameterCountComparison = x.Parameters.Length.CompareTo(y.Parameters.Length);
            if (parameterCountComparison != 0)
                return parameterCountComparison;

            // todo: compare parameter types
            return 0;
        }
    }
}

