using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0077Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0077";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Contents inside braces must be indented one step",
            messageFormat: "Indent content one step from the opening brace",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Contents inside braces must be indented by exactly one step relative to the indentation of the line containing the opening brace.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
            context.RegisterSyntaxNodeAction(AnalyzeAccessorList, SyntaxKind.AccessorList);
            context.RegisterSyntaxNodeAction(
                AnalyzeTypeDeclaration,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.ImplicitObjectCreationExpression,
                SyntaxKind.AnonymousObjectCreationExpression);
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;
            if (block.Statements.Count == 0)
                return;

            if (block.ContainsDirectives)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);
            var indentSize = GetIndentSize(context);

            var declarationFirstToken = block.Parent?.GetFirstToken() ?? block.OpenBraceToken;
            var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
            var declarationLineText = sourceText.Lines[declarationLine].ToString();
            var declarationIndent = CountLeadingWhitespace(declarationLineText);
            var expectedIndent = declarationIndent + indentSize;

            var openBraceLine = tree.GetLineSpan(block.OpenBraceToken.Span).StartLinePosition.Line;

            foreach (var statement in block.Statements)
            {
                var firstToken = statement.GetFirstToken();
                if (firstToken == default)
                    continue;

                var statementLine = tree.GetLineSpan(firstToken.Span).StartLinePosition.Line;
                if (statementLine == openBraceLine)
                    continue;

                var statementLineText = sourceText.Lines[statementLine].ToString();
                var actualIndent = CountLeadingWhitespace(statementLineText);

                if (actualIndent != expectedIndent)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, firstToken.GetLocation()));
                }
            }
        }

        private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (TypeDeclarationSyntax)context.Node;
            if (declaration.Members.Count == 0)
                return;

            if (declaration.ContainsDirectives)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);
            var indentSize = GetIndentSize(context);

            var declarationLine = tree.GetLineSpan(declaration.Identifier.Span).StartLinePosition.Line;
            var declarationLineText = sourceText.Lines[declarationLine].ToString();
            var declarationIndent = CountLeadingWhitespace(declarationLineText);
            var expectedIndent = declarationIndent + indentSize;

            var openBraceLine = tree.GetLineSpan(declaration.OpenBraceToken.Span).StartLinePosition.Line;

            foreach (var member in declaration.Members)
            {
                var firstToken = member.GetFirstToken();
                if (firstToken == default)
                    continue;

                var memberLine = tree.GetLineSpan(firstToken.Span).StartLinePosition.Line;
                if (memberLine == openBraceLine)
                    continue;

                var memberLineText = sourceText.Lines[memberLine].ToString();
                var actualIndent = CountLeadingWhitespace(memberLineText);

                if (actualIndent != expectedIndent)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, firstToken.GetLocation()));
            }
        }

        private static void AnalyzeAccessorList(SyntaxNodeAnalysisContext context)
        {
            var accessorList = (AccessorListSyntax)context.Node;
            if (accessorList.Accessors.Count == 0)
                return;

            if (accessorList.ContainsDirectives)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);
            var indentSize = GetIndentSize(context);

            var declarationFirstToken = accessorList.Parent?.GetFirstToken() ?? accessorList.OpenBraceToken;
            var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
            var declarationLineText = sourceText.Lines[declarationLine].ToString();
            var declarationIndent = CountLeadingWhitespace(declarationLineText);
            var expectedIndent = declarationIndent + indentSize;

            var openBraceLine = tree.GetLineSpan(accessorList.OpenBraceToken.Span).StartLinePosition.Line;

            foreach (var accessor in accessorList.Accessors)
            {
                var firstToken = accessor.GetFirstToken();
                if (firstToken == default)
                    continue;

                var accessorLine = tree.GetLineSpan(firstToken.Span).StartLinePosition.Line;
                if (accessorLine == openBraceLine)
                    continue;

                var accessorLineText = sourceText.Lines[accessorLine].ToString();
                var actualIndent = CountLeadingWhitespace(accessorLineText);

                if (actualIndent != expectedIndent)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, firstToken.GetLocation()));
            }
        }

        private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            SyntaxToken newKeyword;
            SyntaxToken openBrace;
            IEnumerable<SyntaxNode> expressions;

            if (context.Node is ObjectCreationExpressionSyntax objectCreation)
            {
                var initializer = objectCreation.Initializer;
                if (initializer == null ||
                    (!initializer.IsKind(SyntaxKind.ObjectInitializerExpression) &&
                     !initializer.IsKind(SyntaxKind.CollectionInitializerExpression)))
                    return;
                if (initializer.Expressions.Count == 0)
                    return;
                newKeyword = objectCreation.NewKeyword;
                openBrace = initializer.OpenBraceToken;
                expressions = initializer.Expressions;
            }
            else if (context.Node is ImplicitObjectCreationExpressionSyntax implicitCreation)
            {
                var initializer = implicitCreation.Initializer;
                if (initializer == null ||
                    (!initializer.IsKind(SyntaxKind.ObjectInitializerExpression) &&
                     !initializer.IsKind(SyntaxKind.CollectionInitializerExpression)))
                    return;
                if (initializer.Expressions.Count == 0)
                    return;
                newKeyword = implicitCreation.NewKeyword;
                openBrace = initializer.OpenBraceToken;
                expressions = initializer.Expressions;
            }
            else if (context.Node is AnonymousObjectCreationExpressionSyntax anonymousCreation)
            {
                if (anonymousCreation.Initializers.Count == 0)
                    return;
                newKeyword = anonymousCreation.NewKeyword;
                openBrace = anonymousCreation.OpenBraceToken;
                expressions = anonymousCreation.Initializers;
            }
            else
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);
            var indentSize = GetIndentSize(context);

            var newKeywordLine = tree.GetLineSpan(newKeyword.Span).StartLinePosition.Line;
            var declarationIndent = CountLeadingWhitespace(sourceText.Lines[newKeywordLine].ToString());
            var expectedIndent = declarationIndent + indentSize;

            var openBraceLine = tree.GetLineSpan(openBrace.Span).StartLinePosition.Line;

            foreach (var expression in expressions)
            {
                var firstToken = expression.GetFirstToken();
                if (firstToken == default)
                    continue;

                var expressionLine = tree.GetLineSpan(firstToken.Span).StartLinePosition.Line;
                if (expressionLine == openBraceLine)
                    continue;

                var actualIndent = CountLeadingWhitespace(sourceText.Lines[expressionLine].ToString());
                if (actualIndent != expectedIndent)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, firstToken.GetLocation()));
            }
        }

        internal static int CountLeadingWhitespace(string text)
        {
            int count = 0;
            while (count < text.Length && (text[count] == ' ' || text[count] == '\t'))
                count++;
            return count;
        }

        private static int GetIndentSize(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (options.TryGetValue("indent_size", out var value) && int.TryParse(value, out var size) && size > 0)
                return size;
            return 4;
        }
    }
}
