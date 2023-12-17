using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Copycat;

internal static class SyntaxNodeExtensions
{
    public static MethodDeclarationSyntax WithBodyFromTemplate(this MethodDeclarationSyntax method, IMethodSymbol? templateMethodSymbol, string fieldName)
    {
        var template =
            templateMethodSymbol?.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
        
        var innerCall = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(fieldName),
                    IdentifierName(method.Identifier)))
            .WithArgumentList(ArgumentList(SeparatedList(method.ParameterList.Parameters.Select(x => Argument(IdentifierName(x.Identifier))))));
        
        var nameofCall = InvocationExpression(
                IdentifierName(
                    Identifier(
                        TriviaList(),
                        SyntaxKind.NameOfKeyword,
                        "nameof",
                        "nameof",
                        TriviaList())))
            .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList<ArgumentSyntax>(
                        Argument(
                            IdentifierName(method.Identifier.ValueText)))));
        
        if (template == null)
            return method.WithExpressionBody(ArrowExpressionClause(innerCall));

        var actionName = template.ParameterList.Parameters.First().Identifier;
        
        if (template.ExpressionBody != null)
        {
            var expression = template.ExpressionBody;
            expression = expression.ReplaceNodes(
                expression.DescendantNodes().OfType<InvocationExpressionSyntax>(),
                (original, rewritten) =>
                {
                    if (rewritten.Expression is not IdentifierNameSyntax identifierName ||
                        identifierName.Identifier.ValueText != actionName.ValueText)
                        return rewritten;

                    return innerCall;
                });
            return method.WithExpressionBody(expression);
        }

        if (template.Body != null)
        {
            var block = template.Body;
            block = block.ReplaceNodes(
                block.DescendantNodes().OfType<InvocationExpressionSyntax>(),
                (original, rewritten) =>
                {
                    if (rewritten.Expression is not IdentifierNameSyntax identifierName ||
                        identifierName.Identifier.ValueText != actionName.ValueText)
                        return rewritten;

                    return innerCall;
                });
            block = block.ReplaceNodes(
                block.DescendantNodes().OfType<InvocationExpressionSyntax>(),
                (original, rewritten) =>
                {
                    if (rewritten.Expression is not IdentifierNameSyntax nameofIdentifier ||
                        nameofIdentifier.Identifier.ValueText != "nameof" || 
                        rewritten.ArgumentList.Arguments.Count != 1 ||
                        rewritten.ArgumentList.Arguments[0].Expression is not IdentifierNameSyntax argumentIdentifier ||
                        argumentIdentifier.Identifier.ValueText != actionName.ValueText)
                        return rewritten;

                    return nameofCall;
                });
            return method.ReplaceToken(method.ChildTokens().First(x => x.IsKind(SyntaxKind.SemicolonToken)), new SyntaxToken())
                .WithBody(block);
        }

        return method.WithExpressionBody(ArrowExpressionClause(innerCall));
    }
    
    public static SyntaxTriviaList GenerateCrefComment(this IMethodSymbol method)
    {
        // get name with generic parameters MethodName{T1, T2}
        // Get the basic method name
        var methodName = method.Name;

        // Check if there are generic parameters
        if (method.TypeParameters.Length > 0)
        {
            // Append the generic parameters in the format {T1, T2, ...}
            methodName += "{" + string.Join(", ", method.TypeParameters.Select(tp => tp.Name)) + "}";
        }
        
        
        var cref = XmlCrefAttribute(NameMemberCref(IdentifierName(methodName))
            .WithParameters(CrefParameterList(SeparatedList(method.Parameters.Select(x =>
                CrefParameter(IdentifierName(x.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                    .Replace('<', '{').Replace('>', '}'))))))));
        
        return TriviaList(
            Trivia(
                DocumentationCommentTrivia(
                    SyntaxKind.SingleLineDocumentationCommentTrivia,
                    List<XmlNodeSyntax>(
                        new XmlNodeSyntax[]
                        {
                            XmlText("/// "),
                            XmlEmptyElement(XmlName("see"), new SyntaxList<XmlAttributeSyntax>(cref)),
                            XmlText()
                                .WithTextTokens(
                                    TokenList(
                                        XmlTextNewLine(
                                            TriviaList(),
                                            "\n",
                                            "\n",
                                            TriviaList())))
                        }))));
    }
    
    public static bool HasAsyncKeyword(this IMethodSymbol method) =>
        method.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax() is MethodDeclarationSyntax methodSyntax && 
        methodSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.AsyncKeyword));
}