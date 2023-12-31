﻿using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Copycat;

[Generator(LanguageNames.CSharp)]
public class DecoratorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            typeof(DecorateAttribute).FullName!,
            (node, token) => IsPartialClass(node), (syntaxContext, token) =>
            {
                var syntax = (ClassDeclarationSyntax)syntaxContext.TargetNode;
                var symbol = (INamedTypeSymbol)syntaxContext.TargetSymbol;
                
                return new SourceContext(syntax, symbol);
            });
        
        context.RegisterSourceOutput(source, (productionContext, classData) =>
        {
            var sw = Stopwatch.StartNew();
            var (classSyntax, classSymbol) = classData;
            var finder = new SymbolFinder(classSymbol);
            
            var templateSelector = new TemplateSelector(finder.FindTemplates());
            
            var interfaceToDecorate = classSymbol.Interfaces.Single();
            var methodsToImplement = finder.FindNotImplementedMethods(interfaceToDecorate);
            
            
            var gen = classSyntax
                .WithBaseList(null)
                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                .WithMembers(new SyntaxList<MemberDeclarationSyntax>());
            gen = gen.ReplaceTrivia(gen.DescendantTrivia()
                    .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                                     trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)),
                (_, _) => Whitespace(""));

            var fieldName = finder.FindFieldsOrPropertiesOfType(interfaceToDecorate).SingleOrDefault()?.Name;
            if (fieldName == null)
            {
                fieldName = "_decorated";
                gen = gen.AddMembers(
                    FieldDeclaration(
                            VariableDeclaration(
                                    IdentifierName(interfaceToDecorate.ToDisplayString()))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                            Identifier(fieldName)))))
                        .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword))));

                var constructors = finder.FindConstructors();
                if(constructors.IsEmpty)
                {
                    gen = gen.AddMembers(
                        ConstructorDeclaration(classSymbol.Name)
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithParameterList(
                                ParameterList(
                                    SingletonSeparatedList(
                                        Parameter(
                                                Identifier("decorated"))
                                            .WithType(
                                                IdentifierName(interfaceToDecorate.ToDisplayString())))))
                            .WithBody(Block(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName(fieldName),
                                        IdentifierName("decorated"))))));
                }
            }

            var methods = methodsToImplement
                .Select(x => new
                {
                    Method = (MethodDeclarationSyntax) x.DeclaringSyntaxReferences.Single().GetSyntax(),
                    Template = templateSelector.FindTemplateForMethod(x)
                })
                .Select(x =>
                {
                    var template = x.Template;
                    var method = x.Method;
            
                    if (template != null)
                    {
                        method = method.AddModifiers(Token(
                            template.GenerateCrefComment(),
                            SyntaxKind.PublicKeyword,
                            TriviaList()));
            
                        if (template.HasAsyncKeyword())
                            method = method.AddModifiers(Token(SyntaxKind.AsyncKeyword));
                    }
                    else
                    {
                        method = method.AddModifiers(Token(SyntaxKind.PublicKeyword));
                    }
            
                    return method.WithBodyFromTemplate(template, fieldName);
                })
                .ToArray();
            gen = gen.AddMembers(methods.Cast<MemberDeclarationSyntax>().ToArray());

            var cu = CompilationUnit()
                .WithUsings(classSyntax.SyntaxTree.GetCompilationUnitRoot().Usings);

            if (classSymbol.ContainingNamespace is { IsGlobalNamespace: false })
                cu = cu.AddMembers(
                    FileScopedNamespaceDeclaration(ParseName(classSymbol.ContainingNamespace.ToDisplayString()))
                        .WithSemicolonToken(
                            Token(
                                TriviaList(),
                                SyntaxKind.SemicolonToken,
                                TriviaList(LineFeed))));
            
            cu = cu.AddMembers(gen);

            cu = cu.WithLeadingTrivia(cu.GetLeadingTrivia().InsertRange(0, new []
            {
                Comment("// <auto-generated/>"),
                // Comment($"// {DateTime.Now:T} {sw.ElapsedMilliseconds} ms")
            }));
            
            productionContext.AddSource($"{classSymbol.Name}.g.cs", cu.NormalizeWhitespace().ToFullString());
        });
    }
    
    private static bool IsPartialClass(SyntaxNode node) =>
        node is ClassDeclarationSyntax classDeclaration && 
        classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);
    
    private record SourceContext(
        ClassDeclarationSyntax ClassSyntax,
        INamedTypeSymbol ClassSymbol);
}