using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0139Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0139";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Type expression must be on a single line",
            messageFormat: "Write the type expression on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Type expressions must be written on a single line without line breaks.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeGenericName, SyntaxKind.GenericName);
            context.RegisterSyntaxNodeAction(AnalyzeArrayType, SyntaxKind.ArrayType);
            context.RegisterSyntaxNodeAction(AnalyzeNullableType, SyntaxKind.NullableType);
            context.RegisterSyntaxNodeAction(AnalyzePointerType, SyntaxKind.PointerType);
            context.RegisterSyntaxNodeAction(AnalyzeQualifiedName, SyntaxKind.QualifiedName);
            context.RegisterSyntaxNodeAction(AnalyzeTupleType, SyntaxKind.TupleType);
            context.RegisterSyntaxNodeAction(AnalyzeAliasQualifiedName, SyntaxKind.AliasQualifiedName);
        }

        private static void AnalyzeGenericName(SyntaxNodeAnalysisContext context)
        {
            var genericName = (GenericNameSyntax)context.Node;
            var tree = genericName.SyntaxTree;

            var identifierLine = tree.GetLineSpan(genericName.Identifier.Span).EndLinePosition.Line;
            var lessThanLine = tree.GetLineSpan(genericName.TypeArgumentList.LessThanToken.Span).StartLinePosition.Line;

            if (identifierLine != lessThanLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, genericName.GetLocation()));
            }
        }

        private static void AnalyzeArrayType(SyntaxNodeAnalysisContext context)
        {
            var arrayType = (ArrayTypeSyntax)context.Node;
            var tree = arrayType.SyntaxTree;

            var elementTypeLastToken = arrayType.ElementType.GetLastToken();
            var openBracket = arrayType.RankSpecifiers[0].OpenBracketToken;

            var elementTypeLine = tree.GetLineSpan(elementTypeLastToken.Span).EndLinePosition.Line;
            var openBracketLine = tree.GetLineSpan(openBracket.Span).StartLinePosition.Line;

            if (elementTypeLine != openBracketLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, arrayType.GetLocation()));
            }
        }

        private static void AnalyzeNullableType(SyntaxNodeAnalysisContext context)
        {
            var nullableType = (NullableTypeSyntax)context.Node;
            var tree = nullableType.SyntaxTree;

            var elementTypeLine = tree.GetLineSpan(nullableType.ElementType.GetLastToken().Span).EndLinePosition.Line;
            var questionLine = tree.GetLineSpan(nullableType.QuestionToken.Span).StartLinePosition.Line;

            if (elementTypeLine != questionLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, nullableType.GetLocation()));
            }
        }

        private static void AnalyzePointerType(SyntaxNodeAnalysisContext context)
        {
            var pointerType = (PointerTypeSyntax)context.Node;
            var tree = pointerType.SyntaxTree;

            var elementTypeLine = tree.GetLineSpan(pointerType.ElementType.GetLastToken().Span).EndLinePosition.Line;
            var asteriskLine = tree.GetLineSpan(pointerType.AsteriskToken.Span).StartLinePosition.Line;

            if (elementTypeLine != asteriskLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, pointerType.GetLocation()));
            }
        }

        private static void AnalyzeQualifiedName(SyntaxNodeAnalysisContext context)
        {
            var qualifiedName = (QualifiedNameSyntax)context.Node;
            var tree = qualifiedName.SyntaxTree;

            var leftLine = tree.GetLineSpan(qualifiedName.Left.GetLastToken().Span).EndLinePosition.Line;
            var dotLine = tree.GetLineSpan(qualifiedName.DotToken.Span).StartLinePosition.Line;
            var rightLine = tree.GetLineSpan(qualifiedName.Right.GetFirstToken().Span).StartLinePosition.Line;

            if (leftLine != dotLine || dotLine != rightLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, qualifiedName.GetLocation()));
            }
        }

        private static void AnalyzeTupleType(SyntaxNodeAnalysisContext context)
        {
            var tupleType = (TupleTypeSyntax)context.Node;
            var tree = tupleType.SyntaxTree;

            var openParenLine = tree.GetLineSpan(tupleType.OpenParenToken.Span).EndLinePosition.Line;
            var closeParenLine = tree.GetLineSpan(tupleType.CloseParenToken.Span).StartLinePosition.Line;

            if (openParenLine != closeParenLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, tupleType.GetLocation()));
            }
        }

        private static void AnalyzeAliasQualifiedName(SyntaxNodeAnalysisContext context)
        {
            var aliasQualifiedName = (AliasQualifiedNameSyntax)context.Node;
            var tree = aliasQualifiedName.SyntaxTree;

            var aliasLine = tree.GetLineSpan(aliasQualifiedName.Alias.GetLastToken().Span).EndLinePosition.Line;
            var colonColonLine = tree.GetLineSpan(aliasQualifiedName.ColonColonToken.Span).StartLinePosition.Line;

            if (aliasLine != colonColonLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, aliasQualifiedName.GetLocation()));
            }
        }
    }
}
