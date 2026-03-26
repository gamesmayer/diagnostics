using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0052Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0052";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank lines between 'return' and returned expression",
            messageFormat: "Remove the blank line between 'return' and the returned expression",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A return statement with an expression must not contain blank lines between the 'return' keyword and the returned expression.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeReturnStatement, SyntaxKind.ReturnStatement);
        }

        private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
        {
            var returnStatement = (ReturnStatementSyntax)context.Node;
            if (returnStatement.Expression == null)
                return;

            var syntaxTree = returnStatement.SyntaxTree;
            var sourceText = syntaxTree.GetText(context.CancellationToken);

            var returnLine = syntaxTree.GetLineSpan(returnStatement.ReturnKeyword.Span).StartLinePosition.Line;
            var expressionLine = syntaxTree.GetLineSpan(returnStatement.Expression.GetFirstToken().Span).StartLinePosition.Line;

            if (expressionLine <= returnLine + 1)
                return;

            for (var line = returnLine + 1; line < expressionLine; line++)
            {
                var lineText = sourceText.Lines[line].ToString();
                if (!string.IsNullOrWhiteSpace(lineText))
                    continue;

                var lineSpan = sourceText.Lines[line].SpanIncludingLineBreak;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(syntaxTree, lineSpan)));
            }
        }
    }
}
