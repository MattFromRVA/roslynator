// Copyright (c) .NET Foundation and Contributors. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Analysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidImplicitConversionFromDateTimeToDateTimeOffsetAnalyzer : BaseDiagnosticAnalyzer
{
    private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            if (_supportedDiagnostics.IsDefault)
                Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AvoidImplicitConversionFromDateTimeToDateTimeOffset);

            return _supportedDiagnostics;
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(f => AnalyzeEqualsValueClause(f), SyntaxKind.EqualsValueClause);
    }

    private static void AnalyzeEqualsValueClause(SyntaxNodeAnalysisContext context)
    {
        var equalsValueClause = (EqualsValueClauseSyntax)context.Node;

        if (equalsValueClause.ContainsDiagnostics)
            return;

        AnalyzeExpression(context, equalsValueClause.Value);
    }

    private static void AnalyzeExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        if (expression is null)
            return;

        ExpressionSyntax inner = expression.WalkDownParentheses();

        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(inner, context.CancellationToken);

        ITypeSymbol type = typeInfo.Type;
        ITypeSymbol convertedType = typeInfo.ConvertedType;

        if (type is null || convertedType is null)
            return;

        if (type.SpecialType != SpecialType.System_DateTime)
            return;

        if (!convertedType.HasMetadataName(MetadataNames.System_DateTimeOffset))
            return;

        DiagnosticHelpers.ReportDiagnostic(
            context,
            DiagnosticRules.AvoidImplicitConversionFromDateTimeToDateTimeOffset,
            inner);
    }
}
