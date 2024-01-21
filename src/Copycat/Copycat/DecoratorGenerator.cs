﻿using System.Collections.Immutable;
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
            (node, _) => IsPartialClass(node), 
            (syntaxContext, _) => new SourceContext(
                (ClassDeclarationSyntax) syntaxContext.TargetNode, 
                (INamedTypeSymbol) syntaxContext.TargetSymbol, 
                syntaxContext.SemanticModel))
            .WithComparer(new SourceContextComparer());
        
        context.RegisterSourceOutput(source, (productionContext, classData) =>
        {
            void ReportDiagnosticToClass(DiagnosticDescriptor descriptor, params object[] messageArgs) =>
                productionContext.ReportDiagnostic(Diagnostic.Create(descriptor,
                    classData.ClassSyntax.Identifier.GetLocation(), messageArgs));

            try
            {
                // var sw = Stopwatch.StartNew();
                var (classSyntax, classSymbol, semantic) = classData;
            
                var finder = new SymbolFinder(classSymbol, semantic);
                var interfaceToDecorate = classSymbol.Interfaces.Single();
            
                var gen = GenerateEmptyClassDeclaration(classSyntax);

                var uninitialized = finder.FindReadonlyUninitializedFieldsOrProperties();
            
                var fieldName = finder.FindFieldsOrPropertiesOfType(interfaceToDecorate).SingleOrDefault()?.Name;
                if (fieldName == null)
                {
                    fieldName = "_decorated";
                    gen = AddPrivateField(gen, interfaceToDecorate.ToDisplayString(), fieldName);
                    uninitialized = uninitialized.Prepend((interfaceToDecorate.ToDisplayString(), fieldName));
                }

                gen = AddConstructors(gen, finder, uninitialized.ToImmutableArray(), semantic);
                gen = AddProperties(finder, interfaceToDecorate, gen, fieldName);
                gen = AddIndexers(finder, interfaceToDecorate, gen, fieldName, semantic);
                gen = AddEvents(finder, interfaceToDecorate, gen, fieldName);
                gen = AddMethods(finder, interfaceToDecorate, fieldName, gen, ReportDiagnosticToClass);

                var cu = CompilationUnit()
                    .WithUsings(classSyntax.SyntaxTree.GetCompilationUnitRoot().Usings);

                if (classSymbol.ContainingNamespace is { IsGlobalNamespace: false })
                {
                    // get namespace syntax from original class
                    var ns = classSyntax.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().First();
                    cu = cu.AddMembers(
                        ns.WithMembers(List<MemberDeclarationSyntax>().Add(gen)));
                }
                else
                {
                    cu = cu.AddMembers(gen);
                }
                
                cu = cu.WithLeadingTrivia(cu.GetLeadingTrivia().InsertRange(0, new []
                {
                    Comment("// <auto-generated/>"),
                    // Comment($"// {DateTime.Now:T} {sw.ElapsedMilliseconds} ms")
                }));
            
                productionContext.AddSource($"{classSymbol.Name}.g.cs", cu.NormalizeWhitespace().ToFullString());
            }
            catch (Exception e)
            {
                ReportDiagnosticToClass(DiagnosticDescriptors.FailedToGenerateDecorator, 
                    classData.ClassSymbol.Name, e.GetType(), e.Message);
            }
        });
    }

    private static ClassDeclarationSyntax AddIndexers(SymbolFinder finder, INamedTypeSymbol interfaceToDecorate, 
        ClassDeclarationSyntax gen, string fieldName, SemanticModel semantic)
    {
        var indexersToImplement = finder.FindNotImplementedIndexers(interfaceToDecorate);
        if(!indexersToImplement.IsEmpty)
        {
            gen = gen.AddMembers(indexersToImplement
                .Select(x =>
                {
                    var indexerSymbol = x;
                    
                    var indexerSyntax = (IndexerDeclarationSyntax?) indexerSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                    if (indexerSyntax == null)
                    {
                        var raw = indexerSymbol.ToMinimalDisplayString(semantic, 0, SymbolDisplayFormats.Friendly);
                        indexerSyntax = CSharpSyntaxTree.ParseText(raw).GetRoot().DescendantNodesAndSelf()
                            .OfType<IndexerDeclarationSyntax>().First();
                    }
                    
                    var indexerDeclaration = IndexerDeclaration(
                            IdentifierName(indexerSymbol.Type.ToDisplayString()))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                        .WithParameterList(indexerSyntax.ParameterList);
                    
                    var argumentList = BracketedArgumentList()
                        .AddArguments(indexerSyntax.ParameterList.Parameters.Select(p => 
                            Argument(IdentifierName(p.Identifier.Text))).ToArray());

                    var accessors = indexerSyntax.AccessorList!.Accessors.Select(accessor =>
                    {
                        if (accessor.Kind() == SyntaxKind.GetAccessorDeclaration)
                            return accessor.WithExpressionBody(
                                ArrowExpressionClause(
                                    ElementAccessExpression(
                                        IdentifierName(fieldName),
                                        argumentList)))
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                        else
                            return accessor.WithExpressionBody(
                                ArrowExpressionClause(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        ElementAccessExpression(
                                            IdentifierName(fieldName),
                                            argumentList),
                                        IdentifierName("value"))))
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                    }).ToArray();
                    
                    return indexerDeclaration.WithAccessorList(AccessorList().AddAccessors(accessors));
                })
                .Cast<MemberDeclarationSyntax>()
                .ToArray());
        }

        return gen;
    }

    private static ClassDeclarationSyntax AddEvents(SymbolFinder finder, INamedTypeSymbol interfaceToDecorate, 
        ClassDeclarationSyntax gen, string fieldName)
    {
        var eventsToImplement = finder.FindNotImplementedEvents(interfaceToDecorate);
        if(!eventsToImplement.IsEmpty)
        {
            gen = gen.AddMembers(eventsToImplement
                .Select(x =>
                {
                    var eventSymbol = x;
                    var eventDeclaration = EventDeclaration(
                            IdentifierName(eventSymbol.Type.ToDisplayString()),
                            Identifier(eventSymbol.Name))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                        .WithAccessorList(
                            AccessorList(
                                List(
                                    new[]
                                    {
                                        AccessorDeclaration(
                                                SyntaxKind.AddAccessorDeclaration)
                                            .WithExpressionBody(
                                                ArrowExpressionClause(
                                                    AssignmentExpression(
                                                        SyntaxKind.AddAssignmentExpression,
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName(fieldName),
                                                            IdentifierName(eventSymbol.Name)),
                                                        IdentifierName("value"))))
                                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                                        AccessorDeclaration(
                                                SyntaxKind.RemoveAccessorDeclaration)
                                            .WithExpressionBody(
                                                ArrowExpressionClause(
                                                    AssignmentExpression(
                                                        SyntaxKind.SubtractAssignmentExpression,
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName(fieldName),
                                                            IdentifierName(eventSymbol.Name)),
                                                        IdentifierName("value"))))
                                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                                    })));
                    return eventDeclaration;
                })
                .Cast<MemberDeclarationSyntax>()
                .ToArray());
        }

        return gen;
    }

    private static ClassDeclarationSyntax AddProperties(SymbolFinder finder, INamedTypeSymbol interfaceToDecorate,
        ClassDeclarationSyntax gen, string fieldName)
    {
        var propertiesToImplement = finder.FindNotImplementedProperties(interfaceToDecorate);
        if (!propertiesToImplement.IsEmpty)
        {
            gen = gen.AddMembers(propertiesToImplement
                .Select(x =>
                {
                    var accessors = Enumerable.Empty<AccessorDeclarationSyntax>();
                    if (x.GetMethod != null)
                    {
                        accessors = accessors.Append(
                            AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration)
                                .WithExpressionBody(
                                    ArrowExpressionClause(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName(fieldName),
                                            IdentifierName(x.Name))))
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
                    }
                        
                    if (x.SetMethod != null)
                    {
                        accessors = accessors.Append(
                            AccessorDeclaration(
                                    SyntaxKind.SetAccessorDeclaration)
                                .WithExpressionBody(
                                    ArrowExpressionClause(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName(fieldName),
                                                IdentifierName(x.Name)),
                                            IdentifierName("value"))))
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
                    }

                    var property = PropertyDeclaration(
                            IdentifierName(x.Type.ToDisplayString()),
                            Identifier(x.Name))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                        .WithAccessorList(AccessorList(List(accessors)));

                    return property;
                })
                .Cast<MemberDeclarationSyntax>()
                .ToArray());
        }

        return gen;
    }

    private static ClassDeclarationSyntax AddMethods(SymbolFinder finder, INamedTypeSymbol interfaceToDecorate,
        string fieldName, ClassDeclarationSyntax gen, Action<DiagnosticDescriptor, object[]> reportDiagnostic)
    {
        var templates = finder.FindTemplates();
        var accessibleTemplates = templates.Where(x => !x.DeclaringSyntaxReferences.IsEmpty).ToImmutableArray();
        foreach (var template in accessibleTemplates.Except(templates, SymbolEqualityComparer.Default))
        {
            reportDiagnostic(DiagnosticDescriptors.TemplateSourceCodeIsNotAccessible, 
                new object[]{template!.ToDisplayString()});
        }
        
        var templateSelector = new TemplateSelector(accessibleTemplates);
        
        var methodsToImplement = finder.FindNotImplementedMethods(interfaceToDecorate);
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
                    
                    return method.WithBodyFromTemplate(template, fieldName);
                }
                else
                {
                    method = method.AddModifiers(Token(SyntaxKind.PublicKeyword));
                    method = method.WithBodyFromTemplate(template, fieldName);
                    
                    if(!templates.IsEmpty)
                    {
                        reportDiagnostic(DiagnosticDescriptors.NoMatchingTemplateForMethod, 
                            new object[]{method.Identifier.ValueText, interfaceToDecorate.Name, gen.Identifier.ValueText});
                    }

                    return method;
                }
            })
            .ToArray();
        gen = gen.AddMembers(methods.Cast<MemberDeclarationSyntax>().ToArray());
        return gen;
    }

    private static ClassDeclarationSyntax AddPrivateField(ClassDeclarationSyntax gen, string type, string name)
    {
        gen = gen.AddMembers(
            FieldDeclaration(
                    VariableDeclaration(
                            IdentifierName(type))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier(name)))))
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword))));
        return gen;
    }
    
    private  static ClassDeclarationSyntax AddConstructors(ClassDeclarationSyntax gen, SymbolFinder finder,
        ImmutableArray<(string type, string name)> uninitialized, SemanticModel semantic)
    {
        if (uninitialized.IsEmpty)
            return gen;

        var baseConstructors = finder.FindBaseConstructors();
        var sameLevelConstructors = finder.FindConstructors();
        var identifier = gen.Identifier;

        var existing = baseConstructors.AddRange(sameLevelConstructors);
        if (existing.IsEmpty)
        {
            
            var usedNames = new HashSet<string>();
            gen = gen.AddMembers(
                GenerateConstructor(identifier, uninitialized, usedNames));
        }
        else
        {
            gen = gen.AddMembers(
                existing
                    .Select(x =>
                    {
                        var constructor = (ConstructorDeclarationSyntax?) x.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax();
                        if (constructor == null)
                        {
                            var raw = x.ToMinimalDisplayString(semantic, 0, SymbolDisplayFormats.Friendly);
                            constructor = CSharpSyntaxTree.ParseText(raw).GetRoot().DescendantNodesAndSelf()
                                .OfType<ConstructorDeclarationSyntax>().First();
                        }
                        
                        var usedNames = new HashSet<string>(constructor.ParameterList.Parameters.Select(p => p.Identifier.Text));
                        var generated = GenerateConstructor(identifier, uninitialized, usedNames)
                            .AddParameterListParameters(constructor.ParameterList.Parameters.ToArray())
                            .WithInitializer(
                                ConstructorInitializer(baseConstructors.Contains(x) 
                                        ? SyntaxKind.BaseConstructorInitializer 
                                        : SyntaxKind.ThisConstructorInitializer)
                                    .AddArgumentListArguments(constructor.ParameterList.Parameters.Select(p =>
                                        Argument(IdentifierName(p.Identifier.Text))).ToArray()));
                        return generated;
                    })
                    .Cast<MemberDeclarationSyntax>()
                    .ToArray());
        }
        return gen;
    }

    private static ConstructorDeclarationSyntax GenerateConstructor(SyntaxToken identifier, ImmutableArray<(string type, string name)> uninitialized, HashSet<string> usedNames)
    {
        var paramMap = new Dictionary<string, string>();
        return ConstructorDeclaration(identifier)
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(ParameterList().AddParameters(uninitialized.Select(x =>
            {
                var name = x.name;
                        
                if(name[0] == '_') name = name.Substring(1);
                if(char.IsUpper(name[0])) name = char.ToLower(name[0]) + name.Substring(1);
                if (usedNames.Contains(name))
                {
                    // find n-th unused name
                    int i = 1;
                    for (; usedNames.Contains(name + i); i++){}
                    name += i;
                }
                usedNames.Add(name);
                paramMap.Add(x.name, name);
                        
                return Parameter(Identifier(name))
                    .WithType(IdentifierName(x.type));
            }).ToArray()))
            .WithBody(Block().AddStatements(
                uninitialized.Select(x =>
                {
                    var name = paramMap[x.name];
                    return (StatementSyntax) ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(x.name),
                            IdentifierName(name)));
                }).ToArray()));
    }
    
    private static ClassDeclarationSyntax GenerateEmptyClassDeclaration(ClassDeclarationSyntax classSyntax)
    {
        var gen = classSyntax
            .WithBaseList(null)
            .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
            .WithMembers(new SyntaxList<MemberDeclarationSyntax>());
        gen = gen.ReplaceTrivia(gen.DescendantTrivia()
                .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                                 trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)),
            (_, _) => Whitespace(""));
        return gen;
    }

    private static bool IsPartialClass(SyntaxNode node) =>
        node is ClassDeclarationSyntax classDeclaration && 
        classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);
    
    private record SourceContext(
        ClassDeclarationSyntax ClassSyntax,
        INamedTypeSymbol ClassSymbol,
        SemanticModel Semantic);
    
    
    private class InterfaceComparer : IEqualityComparer<INamedTypeSymbol>
    {
        public bool Equals(INamedTypeSymbol? x, INamedTypeSymbol? y)
        {
            if (x == null || y == null)
                return false;

            var format = SymbolDisplayFormats.Detailed;
            
            if (x.ToDisplayString(format) != y.ToDisplayString(format))
                return false;
            
            var xMembers = GetUsefulInterfaceMembers(x).Select(m => m.ToDisplayString(format)).ToArray();
            var yMembers = GetUsefulInterfaceMembers(y).Select(m => m.ToDisplayString(format)).ToArray();
            
            if (xMembers.Length != yMembers.Length)
                return false;
            
            Array.Sort(xMembers);
            Array.Sort(yMembers);
            
            return xMembers.SequenceEqual(yMembers);
        }

        public int GetHashCode(INamedTypeSymbol obj)
        {
            var hash = 17;
            // compare just basic info about the interface
            hash = hash * 23 + obj.Name.GetHashCode();
            foreach (var memberName in obj.MemberNames)
            {
                hash = hash * 23 + memberName.GetHashCode();
            }

            return hash;
        }
        
        // only methods, properties and events are useful for us, it's all we use for generator
        private IEnumerable<ISymbol> GetUsefulInterfaceMembers(INamedTypeSymbol symbol)
        {
            foreach (var member in symbol.GetMembers())
            {
                if(member is IMethodSymbol { MethodKind: MethodKind.Ordinary } method)
                    yield return method;
                else if(member is IPropertySymbol property) // indexers are properties too
                    yield return property;
                else if(member is IEventSymbol @event)
                    yield return @event;
            }
        }
    }
    
    private class SourceContextComparer : IEqualityComparer<SourceContext>
    {
        private readonly InterfaceComparer _interfaceComparer = new();
        
        public bool Equals(SourceContext? x, SourceContext? y)
        {
            if (x == null || y == null)
                return false;

            if (x.ClassSymbol.Interfaces.Length != 1 || y.ClassSymbol.Interfaces.Length != 1)
                return false;

            // class syntax + interface symbol
            return x.ClassSyntax.GetText().ContentEquals(y.ClassSyntax.GetText()) &&
                   _interfaceComparer.Equals(x.ClassSymbol.Interfaces[0], y.ClassSymbol.Interfaces[0]);
        }

        public int GetHashCode(SourceContext obj)
        {
            var hash = 0;
            if(obj.ClassSymbol.Interfaces.Length != 1)
            {
                // no interface - doesn't matter what's in the class
                return hash;
            }
            
            var checksum = obj.ClassSyntax.GetText().GetChecksum();

            // use int from first 4 bytes of checksum (it's SHA1, so it has more than 4 bytes)
            hash = hash << 8 | checksum[0];
            hash = hash << 8 | checksum[1];
            hash = hash << 8 | checksum[2];
            hash = hash << 8 | checksum[3];
            
            hash = hash * 23 + _interfaceComparer.GetHashCode(obj.ClassSymbol.Interfaces[0]);
            return hash;
        }
    }
}