using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

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
            context.RegisterSyntaxNodeAction(AnalyzeElseClause, SyntaxKind.ElseClause);
            context.RegisterSyntaxNodeAction(AnalyzeCatchClause, SyntaxKind.CatchClause);
            context.RegisterSyntaxNodeAction(AnalyzeFinallyClause, SyntaxKind.FinallyClause);
            context.RegisterSyntaxNodeAction(AnalyzeAccessorList, SyntaxKind.AccessorList);
            context.RegisterSyntaxNodeAction(AnalyzeNamespaceDeclaration, SyntaxKind.NamespaceDeclaration);
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
            context.RegisterSyntaxNodeAction(AnalyzeArrayCreation,
                SyntaxKind.ArrayCreationExpression,
                SyntaxKind.ImplicitArrayCreationExpression);
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

                CheckCommentTriviaIndentation(context, tree, sourceText, firstToken.LeadingTrivia, expectedIndent, openBraceLine);

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

            CheckCommentTriviaIndentation(context, tree, sourceText, block.CloseBraceToken.LeadingTrivia, expectedIndent, openBraceLine);
        }

        private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;
            if (elseClause.Parent is not IfStatementSyntax ifStatement)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var ifKeywordLine = tree.GetLineSpan(ifStatement.IfKeyword.Span).StartLinePosition.Line;
            var elseKeywordLine = tree.GetLineSpan(elseClause.ElseKeyword.Span).StartLinePosition.Line;

            if (elseKeywordLine == ifKeywordLine)
                return;

            var expectedIndent = CountLeadingWhitespace(sourceText.Lines[ifKeywordLine].ToString());
            var actualIndent = CountLeadingWhitespace(sourceText.Lines[elseKeywordLine].ToString());

            if (actualIndent != expectedIndent)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, elseClause.ElseKeyword.GetLocation()));
        }

        private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var catchClause = (CatchClauseSyntax)context.Node;
            if (catchClause.Parent is not TryStatementSyntax tryStatement)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var tryKeywordLine = tree.GetLineSpan(tryStatement.TryKeyword.Span).StartLinePosition.Line;
            var catchKeywordLine = tree.GetLineSpan(catchClause.CatchKeyword.Span).StartLinePosition.Line;

            if (catchKeywordLine == tryKeywordLine)
                return;

            var expectedIndent = CountLeadingWhitespace(sourceText.Lines[tryKeywordLine].ToString());
            var actualIndent = CountLeadingWhitespace(sourceText.Lines[catchKeywordLine].ToString());

            if (actualIndent != expectedIndent)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, catchClause.CatchKeyword.GetLocation()));
        }

        private static void AnalyzeFinallyClause(SyntaxNodeAnalysisContext context)
        {
            var finallyClause = (FinallyClauseSyntax)context.Node;
            if (finallyClause.Parent is not TryStatementSyntax tryStatement)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var tryKeywordLine = tree.GetLineSpan(tryStatement.TryKeyword.Span).StartLinePosition.Line;
            var finallyKeywordLine = tree.GetLineSpan(finallyClause.FinallyKeyword.Span).StartLinePosition.Line;

            if (finallyKeywordLine == tryKeywordLine)
                return;

            var expectedIndent = CountLeadingWhitespace(sourceText.Lines[tryKeywordLine].ToString());
            var actualIndent = CountLeadingWhitespace(sourceText.Lines[finallyKeywordLine].ToString());

            if (actualIndent != expectedIndent)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, finallyClause.FinallyKeyword.GetLocation()));
        }

        private static void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var ns = (NamespaceDeclarationSyntax)context.Node;
            if (ns.Members.Count == 0)
                return;

            if (HasDirectivesInMemberList(ns.Members, ns.CloseBraceToken))
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);
            var indentSize = GetIndentSize(context);

            var declarationLine = tree.GetLineSpan(ns.NamespaceKeyword.Span).StartLinePosition.Line;
            var declarationIndent = CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());
            var expectedIndent = declarationIndent + indentSize;

            var openBraceLine = tree.GetLineSpan(ns.OpenBraceToken.Span).StartLinePosition.Line;

            foreach (var member in ns.Members)
                CheckMemberIndentation(context, tree, sourceText, member, expectedIndent, openBraceLine);
        }

        private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (TypeDeclarationSyntax)context.Node;
            if (declaration.Members.Count == 0)
                return;

            if (HasDirectivesInMemberList(declaration.Members, declaration.CloseBraceToken))
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
                CheckMemberIndentation(context, tree, sourceText, member, expectedIndent, openBraceLine);
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

        private static void AnalyzeArrayCreation(SyntaxNodeAnalysisContext context)
        {
            InitializerExpressionSyntax? initializer;

            if (context.Node is ArrayCreationExpressionSyntax arrayCreation)
                initializer = arrayCreation.Initializer;
            else if (context.Node is ImplicitArrayCreationExpressionSyntax implicitArrayCreation)
                initializer = implicitArrayCreation.Initializer;
            else
                return;

            if (initializer == null || initializer.Expressions.Count == 0)
                return;

            if (initializer.ContainsDirectives)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);
            var indentSize = GetIndentSize(context);

            var declarationFirstToken = context.Node.GetFirstToken();
            var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
            var declarationIndent = CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());
            var expectedIndent = declarationIndent + indentSize;

            var openBraceLine = tree.GetLineSpan(initializer.OpenBraceToken.Span).StartLinePosition.Line;

            foreach (var expression in initializer.Expressions)
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

        private static bool HasDirectivesInMemberList<T>(SyntaxList<T> members, SyntaxToken closeBrace)
            where T : MemberDeclarationSyntax
        {
            foreach (var trivia in closeBrace.LeadingTrivia)
                if (IsConditionalDirective(trivia)) return true;
            foreach (var member in members)
                foreach (var trivia in member.GetFirstToken().LeadingTrivia)
                    if (IsConditionalDirective(trivia)) return true;
            return false;
        }

        private static bool IsConditionalDirective(SyntaxTrivia trivia) =>
            trivia.IsKind(SyntaxKind.IfDirectiveTrivia) ||
            trivia.IsKind(SyntaxKind.ElifDirectiveTrivia) ||
            trivia.IsKind(SyntaxKind.ElseDirectiveTrivia) ||
            trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia);

        private static void CheckMemberIndentation(
            SyntaxNodeAnalysisContext context,
            SyntaxTree tree,
            SourceText sourceText,
            MemberDeclarationSyntax member,
            int expectedIndent,
            int openBraceLine)
        {
            foreach (var attrList in member.AttributeLists)
            {
                var token = attrList.OpenBracketToken;
                var line = tree.GetLineSpan(token.Span).StartLinePosition.Line;
                if (line == openBraceLine)
                    continue;
                var actualIndent = CountLeadingWhitespace(sourceText.Lines[line].ToString());
                if (actualIndent != expectedIndent)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, token.GetLocation()));
            }

            var firstNonAttr = member.AttributeLists.Count > 0
                ? member.AttributeLists.Last().GetLastToken().GetNextToken()
                : member.GetFirstToken();

            if (firstNonAttr == default || firstNonAttr.IsMissing)
                return;

            var firstNonAttrLine = tree.GetLineSpan(firstNonAttr.Span).StartLinePosition.Line;
            if (firstNonAttrLine == openBraceLine)
                return;

            var firstNonAttrIndent = CountLeadingWhitespace(sourceText.Lines[firstNonAttrLine].ToString());
            if (firstNonAttrIndent != expectedIndent)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, firstNonAttr.GetLocation()));
        }

        private static void CheckCommentTriviaIndentation(
            SyntaxNodeAnalysisContext context,
            SyntaxTree tree,
            SourceText sourceText,
            SyntaxTriviaList triviaList,
            int expectedIndent,
            int openBraceLine)
        {
            foreach (var trivia in triviaList)
            {
                if (!trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) &&
                    !trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                    continue;

                var triviaLine = tree.GetLineSpan(trivia.Span).StartLinePosition.Line;
                if (triviaLine == openBraceLine)
                    continue;

                var actualIndent = CountLeadingWhitespace(sourceText.Lines[triviaLine].ToString());
                if (actualIndent != expectedIndent)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, trivia.GetLocation()));
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
