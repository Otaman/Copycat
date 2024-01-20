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
        
        return candidates.OrderBy(x => x, new TemplateCandidatesComparer(method)).First();
    }
    
    // naive implementation
    private static bool IsCandidate(IMethodSymbol template, IMethodSymbol method)
    {
        // has compatible return type
        if(method.ReturnsVoid && !template.ReturnsVoid || !template.IsGenericMethod && !SymbolEqualityComparer.Default.Equals(template.ReturnType, method.ReturnType))
            return false;
        
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
        
            // the more parameters the better
            var parameterCountComparison = x.Parameters.Length.CompareTo(y.Parameters.Length);
            if (parameterCountComparison != 0)
                return parameterCountComparison;

            // todo: compare parameter types
            return 0;
        }
    }
}

