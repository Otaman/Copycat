using Microsoft.CodeAnalysis;

namespace Copycat;

internal static class SymbolDisplayFormats
{
    public static SymbolDisplayFormat Detailed { get; } = new (
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        propertyStyle: SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
        localOptions: SymbolDisplayLocalOptions.IncludeType,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
        memberOptions:
        SymbolDisplayMemberOptions.IncludeParameters |
        SymbolDisplayMemberOptions.IncludeContainingType |
        SymbolDisplayMemberOptions.IncludeType |
        SymbolDisplayMemberOptions.IncludeRef |
        SymbolDisplayMemberOptions.IncludeExplicitInterface,
        kindOptions:
        SymbolDisplayKindOptions.IncludeMemberKeyword,
        parameterOptions:
        SymbolDisplayParameterOptions.IncludeOptionalBrackets |
        SymbolDisplayParameterOptions.IncludeDefaultValue |
        SymbolDisplayParameterOptions.IncludeParamsRefOut |
        SymbolDisplayParameterOptions.IncludeExtensionThis |
        SymbolDisplayParameterOptions.IncludeType |
        SymbolDisplayParameterOptions.IncludeName,
        miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
        SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
    
    public static SymbolDisplayFormat Friendly { get; } = new (
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
        propertyStyle: SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
        localOptions: SymbolDisplayLocalOptions.IncludeType,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
        memberOptions:
        SymbolDisplayMemberOptions.IncludeParameters |
        SymbolDisplayMemberOptions.IncludeType |
        SymbolDisplayMemberOptions.IncludeRef |
        SymbolDisplayMemberOptions.IncludeExplicitInterface,
        kindOptions:
        SymbolDisplayKindOptions.IncludeMemberKeyword,
        parameterOptions:
        SymbolDisplayParameterOptions.IncludeOptionalBrackets |
        SymbolDisplayParameterOptions.IncludeDefaultValue |
        SymbolDisplayParameterOptions.IncludeParamsRefOut |
        SymbolDisplayParameterOptions.IncludeExtensionThis |
        SymbolDisplayParameterOptions.IncludeType |
        SymbolDisplayParameterOptions.IncludeName,
        miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
        SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
        SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
    
    public static SymbolDisplayFormat DetailedNoReturnType { get; } = new (
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        propertyStyle: SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
        localOptions: SymbolDisplayLocalOptions.IncludeType,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
        memberOptions:
        SymbolDisplayMemberOptions.IncludeParameters |
        SymbolDisplayMemberOptions.IncludeRef |
        SymbolDisplayMemberOptions.IncludeExplicitInterface,
        kindOptions:
        SymbolDisplayKindOptions.IncludeMemberKeyword,
        parameterOptions:
        SymbolDisplayParameterOptions.IncludeOptionalBrackets |
        SymbolDisplayParameterOptions.IncludeDefaultValue |
        SymbolDisplayParameterOptions.IncludeParamsRefOut |
        SymbolDisplayParameterOptions.IncludeExtensionThis |
        SymbolDisplayParameterOptions.IncludeType |
        SymbolDisplayParameterOptions.IncludeName,
        miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
        SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
}