// Copyright (c) .NET Foundation and Contributors. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
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
        context.RegisterSyntaxNodeAction(f => AnalyzeSimpleAssignment(f), SyntaxKind.SimpleAssignmentExpression);
        context.RegisterSyntaxNodeAction(f => AnalyzeArgument(f), SyntaxKind.Argument);
        context.RegisterSyntaxNodeAction(f => AnalyzeReturnStatement(f), SyntaxKind.ReturnStatement);
        context.RegisterSyntaxNodeAction(f => AnalyzeArrowExpressionClause(f), SyntaxKind.ArrowExpressionClause);
    }

    private static void AnalyzeEqualsValueClause(SyntaxNodeAnalysisContext context)
    {
        var equalsValueClause = (EqualsValueClauseSyntax)context.Node;

        if (equalsValueClause.ContainsDiagnostics)
            return;

        AnalyzeExpression(context, equalsValueClause.Value);
    }

    private static void AnalyzeSimpleAssignment(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;

        if (assignment.ContainsDiagnostics)
            return;

        AnalyzeExpression(context, assignment.Right);
    }

    private static void AnalyzeArgument(SyntaxNodeAnalysisContext context)
    {
        var argument = (ArgumentSyntax)context.Node;

        if (argument.ContainsDiagnostics)
            return;

        AnalyzeExpression(context, argument.Expression);
    }

    private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
    {
        var returnStatement = (ReturnStatementSyntax)context.Node;

        if (returnStatement.ContainsDiagnostics)
            return;

        AnalyzeExpression(context, returnStatement.Expression);
    }

    private static void AnalyzeArrowExpressionClause(SyntaxNodeAnalysisContext context)
    {
        var arrow = (ArrowExpressionClauseSyntax)context.Node;

        if (arrow.ContainsDiagnostics)
            return;

        AnalyzeExpression(context, arrow.Expression);
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

        if (IsKnownSafeKind(inner, context.SemanticModel, context.CancellationToken))
            return;

        DiagnosticHelpers.ReportDiagnostic(
            context,
            DiagnosticRules.AvoidImplicitConversionFromDateTimeToDateTimeOffset,
            inner);
    }

    private static bool IsKnownSafeKind(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        // 'DateTime.UtcNow' / 'DateTime.Now' / 'DateTime.Today' (properties) / 'DateTime.UnixEpoch' (static field)
        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            ISymbol member = semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol;
            if (member is IPropertySymbol prop
                && prop.IsStatic
                && prop.ContainingType?.SpecialType == SpecialType.System_DateTime)
            {
                switch (prop.Name)
                {
                    case "UtcNow":
                    case "Now":
                    case "Today":
                        return true;
                }
            }

            if (member is IFieldSymbol field
                && field.IsStatic
                && field.ContainingType?.SpecialType == SpecialType.System_DateTime
                && field.Name == "UnixEpoch")
            {
                return true;
            }
        }

        // 'DateTime.SpecifyKind(x, DateTimeKind.Utc/Local)', 'expr.ToUniversalTime()', 'expr.ToLocalTime()'
        if (expression is InvocationExpressionSyntax invocation)
        {
            ISymbol invokedSymbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol;
            if (invokedSymbol is IMethodSymbol method
                && method.ContainingType?.SpecialType == SpecialType.System_DateTime)
            {
                switch (method.Name)
                {
                    case "ToUniversalTime":
                    case "ToLocalTime":
                        return true;
                    case "SpecifyKind":
                        return ArgumentListHasLiteralUtcOrLocalKind(invocation.ArgumentList, semanticModel, cancellationToken);
                }
            }
        }

        // 'new DateTime(..., DateTimeKind.Utc/Local)'
        if (expression is ObjectCreationExpressionSyntax creation)
        {
            ISymbol ctorSymbol = semanticModel.GetSymbolInfo(creation, cancellationToken).Symbol;
            if (ctorSymbol is IMethodSymbol ctor
                && ctor.MethodKind == MethodKind.Constructor
                && ctor.ContainingType?.SpecialType == SpecialType.System_DateTime)
            {
                return ArgumentListHasLiteralUtcOrLocalKind(creation.ArgumentList, semanticModel, cancellationToken);
            }
        }

        return false;
    }

    // Scans an argument list for one whose expression has type 'System.DateTimeKind' and is a literal
    // 'DateTimeKind.Utc' or 'DateTimeKind.Local' member access. Robust to named-argument reordering.
    private static bool ArgumentListHasLiteralUtcOrLocalKind(ArgumentListSyntax args, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (args is null)
            return false;

        foreach (ArgumentSyntax argument in args.Arguments)
        {
            ITypeSymbol argType = semanticModel.GetTypeInfo(argument.Expression, cancellationToken).Type;
            if (argType?.Name == "DateTimeKind"
                && argType.ContainingNamespace?.Name == "System"
                && argType.ContainingNamespace.ContainingNamespace?.IsGlobalNamespace == true)
            {
                return ArgumentIsLiteralUtcOrLocal(argument, semanticModel, cancellationToken);
            }
        }

        return false;
    }

    private static bool ArgumentIsLiteralUtcOrLocal(ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (argument.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        ISymbol symbol = semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol;
        if (symbol is not IFieldSymbol field || !field.IsStatic)
            return false;

        INamedTypeSymbol containingType = field.ContainingType;
        if (containingType?.Name != "DateTimeKind"
            || containingType.ContainingNamespace?.Name != "System"
            || containingType.ContainingNamespace.ContainingNamespace?.IsGlobalNamespace != true)
        {
            return false;
        }

        return field.Name is "Utc" or "Local";
    }
}
