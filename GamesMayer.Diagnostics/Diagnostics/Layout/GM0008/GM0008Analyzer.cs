using System.Collections.Immutable;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0008Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0008";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Blank line within argument or parameter list",
            messageFormat: "The blank line within the argument or parameter list must be removed",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When invocation arguments or method declaration parameters are split across multiple lines, they must appear as a continuous block without blank lines.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeImplicitObjectCreation, SyntaxKind.ImplicitObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConstructorInitializer, SyntaxKind.ThisConstructorInitializer);
            context.RegisterSyntaxNodeAction(AnalyzeConstructorInitializer, SyntaxKind.BaseConstructorInitializer);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var argumentList = invocation.ArgumentList;
            AnalyzeList(
                context,
                argumentList.Arguments,
                argumentList.OpenParenToken,
                argumentList.CloseParenToken);
        }

        private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
            if (objectCreation.ArgumentList == null)
                return;
            var argumentList = objectCreation.ArgumentList;
            AnalyzeList(
                context,
                argumentList.Arguments,
                argumentList.OpenParenToken,
                argumentList.CloseParenToken);
        }

        private static void AnalyzeImplicitObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var implicitCreation = (ImplicitObjectCreationExpressionSyntax)context.Node;
            var argumentList = implicitCreation.ArgumentList;
            AnalyzeList(
                context,
                argumentList.Arguments,
                argumentList.OpenParenToken,
                argumentList.CloseParenToken);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            var parameterList = methodDeclaration.ParameterList;
            AnalyzeList(
                context,
                parameterList.Parameters,
                parameterList.OpenParenToken,
                parameterList.CloseParenToken);
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;
            var parameterList = constructorDeclaration.ParameterList;
            AnalyzeList(
                context,
                parameterList.Parameters,
                parameterList.OpenParenToken,
                parameterList.CloseParenToken);
        }

        private static void AnalyzeConstructorInitializer(SyntaxNodeAnalysisContext context)
        {
            var initializer = (ConstructorInitializerSyntax)context.Node;
            var argumentList = initializer.ArgumentList;
            AnalyzeList(
                context,
                argumentList.Arguments,
                argumentList.OpenParenToken,
                argumentList.CloseParenToken);
        }

        private static void AnalyzeList<TNode>(
            SyntaxNodeAnalysisContext context,
            SeparatedSyntaxList<TNode> items,
            SyntaxToken openParenToken,
            SyntaxToken closeParenToken)
            where TNode : SyntaxNode
        {
            if (items.Count == 0)
                return;

            int openParenLine = openParenToken.GetLocation().GetLineSpan().EndLinePosition.Line;
            int closeParenLine = closeParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;
            if (openParenLine == closeParenLine)
                return;

            var syntaxTree = context.Node.SyntaxTree;
            var text = syntaxTree.GetText(context.CancellationToken);

            var firstItemStartLine = items[0].GetFirstToken().GetLocation().GetLineSpan().StartLinePosition.Line;
            ReportBlankLineDiagnostics(context, syntaxTree, text, openParenLine + 1, firstItemStartLine - 1);

            for (int i = 1; i < items.Count; i++)
            {
                int previousEndLine = items[i - 1].GetLastToken().GetLocation().GetLineSpan().EndLinePosition.Line;
                int currentStartLine = items[i].GetFirstToken().GetLocation().GetLineSpan().StartLinePosition.Line;
                ReportBlankLineDiagnostics(context, syntaxTree, text, previousEndLine + 1, currentStartLine - 1);
            }

            int lastItemEndLine = items[items.Count - 1].GetLastToken().GetLocation().GetLineSpan().EndLinePosition.Line;
            ReportBlankLineDiagnostics(context, syntaxTree, text, lastItemEndLine + 1, closeParenLine - 1);
        }

        private static void ReportBlankLineDiagnostics(
            SyntaxNodeAnalysisContext context,
            SyntaxTree syntaxTree,
            SourceText text,
            int startLine,
            int endLine)
        {
            if (startLine > endLine)
                return;

            if (startLine < 0)
                startLine = 0;

            if (endLine >= text.Lines.Count)
                endLine = text.Lines.Count - 1;

            if (startLine > endLine)
                return;

            for (int line = startLine; line <= endLine; line++)
            {
                var lineText = text.Lines[line].ToString();
                if (!string.IsNullOrWhiteSpace(lineText))
                    continue;

                var lineSpan = text.Lines[line].SpanIncludingLineBreak;
                if (lineSpan.Length == 0)
                    lineSpan = text.Lines[line].Span;

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    Location.Create(syntaxTree, lineSpan)));
            }
        }
    }
}
