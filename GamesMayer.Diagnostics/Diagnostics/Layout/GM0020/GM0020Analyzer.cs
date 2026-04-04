using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using GamesMayer.Diagnostics.Utils;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0020Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0020";
        private const string OptionKey = "dotnet_diagnostic.GM0020";
        private const string StyleOptionKey = "dotnet_diagnostic.GM0020.style";
        internal const string StylePropertyKey = "style";
        internal const string BraceKindPropertyKey = "brace_kind";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Brace placement must match the configured style for non-empty enclosures",
            messageFormat: "Place the {0} brace {1}",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Opening and closing braces for non-empty enclosures must match the configured brace style.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.NamespaceDeclaration,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.IndexerDeclaration,
                SyntaxKind.EventDeclaration,
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKind.InitAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.LocalFunctionStatement,
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.IfStatement,
                SyntaxKind.ElseClause,
                SyntaxKind.ForStatement,
                SyntaxKind.ForEachStatement,
                SyntaxKind.ForEachVariableStatement,
                SyntaxKind.WhileStatement,
                SyntaxKind.DoStatement,
                SyntaxKind.UsingStatement,
                SyntaxKind.LockStatement,
                SyntaxKind.FixedStatement,
                SyntaxKind.CheckedStatement,
                SyntaxKind.UnsafeStatement,
                SyntaxKind.SwitchStatement,
                SyntaxKind.SwitchSection,
                SyntaxKind.TryStatement,
                SyntaxKind.CatchClause,
                SyntaxKind.FinallyClause,
                SyntaxKind.ObjectInitializerExpression,
                SyntaxKind.CollectionInitializerExpression,
                SyntaxKind.ArrayInitializerExpression,
                SyntaxKind.AnonymousObjectCreationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var configuredCategories = GetConfiguredCategories(context);
            if (configuredCategories == BraceCategory.None)
            {
                return;
            }

            var configuredStyle = GetConfiguredStyle(context);

            switch (context.Node)
            {
                case NamespaceDeclarationSyntax namespaceDeclaration:
                    AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Types, namespaceDeclaration.OpenBraceToken, namespaceDeclaration.CloseBraceToken, namespaceDeclaration.Members.Count == 0);
                    break;
                case TypeDeclarationSyntax typeDeclaration:
                    AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Types, typeDeclaration.OpenBraceToken, typeDeclaration.CloseBraceToken, typeDeclaration.Members.Count == 0);
                    break;
                case EnumDeclarationSyntax enumDeclaration:
                    AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Types, enumDeclaration.OpenBraceToken, enumDeclaration.CloseBraceToken, enumDeclaration.Members.Count == 0);
                    break;
                case PropertyDeclarationSyntax propertyDeclaration:
                    if (propertyDeclaration.AccessorList != null)
                    {
                        var isAutoImplemented = propertyDeclaration.AccessorList.Accessors.All(
                            a => a.Body == null && a.ExpressionBody == null);
                        if (!isAutoImplemented)
                        {
                            AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Properties, propertyDeclaration.AccessorList.OpenBraceToken, propertyDeclaration.AccessorList.CloseBraceToken, propertyDeclaration.AccessorList.Accessors.Count == 0);
                        }
                    }

                    break;
                case IndexerDeclarationSyntax indexerDeclaration:
                    if (indexerDeclaration.AccessorList != null)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Indexers, indexerDeclaration.AccessorList.OpenBraceToken, indexerDeclaration.AccessorList.CloseBraceToken, indexerDeclaration.AccessorList.Accessors.Count == 0);
                    }

                    break;
                case EventDeclarationSyntax eventDeclaration:
                    if (eventDeclaration.AccessorList != null)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Events, eventDeclaration.AccessorList.OpenBraceToken, eventDeclaration.AccessorList.CloseBraceToken, eventDeclaration.AccessorList.Accessors.Count == 0);
                    }

                    break;
                case AccessorDeclarationSyntax accessorDeclaration:
                    if (accessorDeclaration.Body != null)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Accessors, accessorDeclaration.Body.OpenBraceToken, accessorDeclaration.Body.CloseBraceToken, accessorDeclaration.Body.Statements.Count == 0);
                    }

                    break;
                case MethodDeclarationSyntax methodDeclaration:
                    if (methodDeclaration.Body != null)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Methods, methodDeclaration.Body.OpenBraceToken, methodDeclaration.Body.CloseBraceToken, methodDeclaration.Body.Statements.Count == 0);
                    }

                    break;
                case ConstructorDeclarationSyntax constructorDeclaration:
                    if (constructorDeclaration.Body != null)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Methods, constructorDeclaration.Body.OpenBraceToken, constructorDeclaration.Body.CloseBraceToken, constructorDeclaration.Body.Statements.Count == 0);
                    }

                    break;
                case DestructorDeclarationSyntax destructorDeclaration:
                    if (destructorDeclaration.Body != null)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Methods, destructorDeclaration.Body.OpenBraceToken, destructorDeclaration.Body.CloseBraceToken, destructorDeclaration.Body.Statements.Count == 0);
                    }

                    break;
                case OperatorDeclarationSyntax operatorDeclaration:
                    if (operatorDeclaration.Body != null)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Methods, operatorDeclaration.Body.OpenBraceToken, operatorDeclaration.Body.CloseBraceToken, operatorDeclaration.Body.Statements.Count == 0);
                    }

                    break;
                case ConversionOperatorDeclarationSyntax conversionOperatorDeclaration:
                    if (conversionOperatorDeclaration.Body != null)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Methods, conversionOperatorDeclaration.Body.OpenBraceToken, conversionOperatorDeclaration.Body.CloseBraceToken, conversionOperatorDeclaration.Body.Statements.Count == 0);
                    }

                    break;
                case LocalFunctionStatementSyntax localFunctionStatement:
                    if (localFunctionStatement.Body != null)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.LocalFunctions, localFunctionStatement.Body.OpenBraceToken, localFunctionStatement.Body.CloseBraceToken, localFunctionStatement.Body.Statements.Count == 0);
                    }

                    break;
                case AnonymousMethodExpressionSyntax anonymousMethodExpression:
                    if (anonymousMethodExpression.Block != null)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.AnonymousMethods, anonymousMethodExpression.Block.OpenBraceToken, anonymousMethodExpression.Block.CloseBraceToken, anonymousMethodExpression.Block.Statements.Count == 0);
                    }

                    break;
                case ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpression:
                    if (parenthesizedLambdaExpression.Body is BlockSyntax parenthesizedLambdaBody)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Lambdas, parenthesizedLambdaBody.OpenBraceToken, parenthesizedLambdaBody.CloseBraceToken, parenthesizedLambdaBody.Statements.Count == 0);
                    }

                    break;
                case SimpleLambdaExpressionSyntax simpleLambdaExpression:
                    if (simpleLambdaExpression.Body is BlockSyntax simpleLambdaBody)
                    {
                        AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.Lambdas, simpleLambdaBody.OpenBraceToken, simpleLambdaBody.CloseBraceToken, simpleLambdaBody.Statements.Count == 0);
                    }

                    break;
                case IfStatementSyntax ifStatement:
                    AnalyzeControlBlock(context, configuredCategories, configuredStyle, ifStatement.Statement);
                    break;
                case ElseClauseSyntax elseClause:
                    AnalyzeControlBlock(context, configuredCategories, configuredStyle, elseClause.Statement);
                    break;
                case ForStatementSyntax forStatement:
                    AnalyzeControlBlock(context, configuredCategories, configuredStyle, forStatement.Statement);
                    break;
                case ForEachStatementSyntax forEachStatement:
                    AnalyzeControlBlock(context, configuredCategories, configuredStyle, forEachStatement.Statement);
                    break;
                case ForEachVariableStatementSyntax forEachVariableStatement:
                    AnalyzeControlBlock(context, configuredCategories, configuredStyle, forEachVariableStatement.Statement);
                    break;
                case WhileStatementSyntax whileStatement:
                    AnalyzeControlBlock(context, configuredCategories, configuredStyle, whileStatement.Statement);
                    break;
                case DoStatementSyntax doStatement:
                    AnalyzeControlBlock(context, configuredCategories, configuredStyle, doStatement.Statement);
                    break;
                case UsingStatementSyntax usingStatement:
                    AnalyzeControlBlock(context, configuredCategories, configuredStyle, usingStatement.Statement);
                    break;
                case LockStatementSyntax lockStatement:
                    AnalyzeControlBlock(context, configuredCategories, configuredStyle, lockStatement.Statement);
                    break;
                case FixedStatementSyntax fixedStatement:
                    AnalyzeControlBlock(context, configuredCategories, configuredStyle, fixedStatement.Statement);
                    break;
                case CheckedStatementSyntax checkedStatement:
                    AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.ControlBlocks, checkedStatement.Block.OpenBraceToken, checkedStatement.Block.CloseBraceToken, checkedStatement.Block.Statements.Count == 0);
                    break;
                case UnsafeStatementSyntax unsafeStatement:
                    AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.ControlBlocks, unsafeStatement.Block.OpenBraceToken, unsafeStatement.Block.CloseBraceToken, unsafeStatement.Block.Statements.Count == 0);
                    break;
                case SwitchStatementSyntax switchStatement:
                    AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.ControlBlocks, switchStatement.OpenBraceToken, switchStatement.CloseBraceToken, switchStatement.Sections.Count == 0);
                    break;
                case SwitchSectionSyntax switchSection:
                    foreach (var statement in switchSection.Statements)
                    {
                        if (statement is BlockSyntax block)
                        {
                            AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.ControlBlocks, block.OpenBraceToken, block.CloseBraceToken, block.Statements.Count == 0);
                        }
                    }

                    break;
                case TryStatementSyntax tryStatement:
                    AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.ControlBlocks, tryStatement.Block.OpenBraceToken, tryStatement.Block.CloseBraceToken, tryStatement.Block.Statements.Count == 0);
                    break;
                case CatchClauseSyntax catchClause:
                    AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.ControlBlocks, catchClause.Block.OpenBraceToken, catchClause.Block.CloseBraceToken, catchClause.Block.Statements.Count == 0);
                    break;
                case FinallyClauseSyntax finallyClause:
                    AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.ControlBlocks, finallyClause.Block.OpenBraceToken, finallyClause.Block.CloseBraceToken, finallyClause.Block.Statements.Count == 0);
                    break;
                case InitializerExpressionSyntax initializerExpression:
                    AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.ObjectCollectionArrayInitializers, initializerExpression.OpenBraceToken, initializerExpression.CloseBraceToken, initializerExpression.Expressions.Count == 0);
                    break;
                case AnonymousObjectCreationExpressionSyntax anonymousObjectCreation:
                    AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.AnonymousTypes, anonymousObjectCreation.OpenBraceToken, anonymousObjectCreation.CloseBraceToken, anonymousObjectCreation.Initializers.Count == 0);
                    break;
            }
        }

        private static void AnalyzeControlBlock(
            SyntaxNodeAnalysisContext context,
            BraceCategory configuredCategories,
            BraceStyle configuredStyle,
            StatementSyntax statement)
        {
            if (statement is BlockSyntax block)
            {
                AnalyzeBraces(context, configuredCategories, configuredStyle, BraceCategory.ControlBlocks, block.OpenBraceToken, block.CloseBraceToken, block.Statements.Count == 0);
            }
        }

        private static void AnalyzeBraces(
            SyntaxNodeAnalysisContext context,
            BraceCategory configuredCategories,
            BraceStyle configuredStyle,
            BraceCategory category,
            SyntaxToken openBrace,
            SyntaxToken closeBrace,
            bool isEmptyEnclosure)
        {
            if (openBrace.IsMissing || closeBrace.IsMissing || isEmptyEnclosure || (configuredCategories & category) == 0)
            {
                return;
            }

            var previousOpenToken = openBrace.GetPreviousToken(includeZeroWidth: false);
            if (previousOpenToken == default)
            {
                return;
            }

            var tree = context.Node.SyntaxTree;
            var previousTokenLine = tree.GetLineSpan(previousOpenToken.Span).EndLinePosition.Line;
            var openBraceLine = tree.GetLineSpan(openBrace.Span).StartLinePosition.Line;
            var isBraceOnDeclarationLine = previousTokenLine == openBraceLine;

            if (IsViolation(configuredStyle, isBraceOnDeclarationLine))
            {
                ReportDiagnostic(context, openBrace, configuredStyle, "opening", GetOpeningBracePlacementText(configuredStyle));
            }

            var previousCloseToken = closeBrace.GetPreviousToken(includeZeroWidth: false);
            if (previousCloseToken == default)
            {
                return;
            }

            var previousCloseTokenLine = tree.GetLineSpan(previousCloseToken.Span).EndLinePosition.Line;
            var closeBraceLine = tree.GetLineSpan(closeBrace.Span).StartLinePosition.Line;
            if (previousCloseTokenLine == closeBraceLine)
            {
                ReportDiagnostic(context, closeBrace, configuredStyle, "closing", "on a new line");
            }
        }

        private static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            SyntaxToken braceToken,
            BraceStyle configuredStyle,
            string braceKind,
            string requiredPlacement)
        {
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(StylePropertyKey, AnalyzerConfigCategoryParser.GetBraceStyleDisplayName(configuredStyle))
                .Add(BraceKindPropertyKey, braceKind);

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Descriptor,
                    braceToken.GetLocation(),
                    properties,
                    braceKind,
                    requiredPlacement));
        }

        private static bool IsViolation(BraceStyle configuredStyle, bool isBraceOnDeclarationLine)
        {
            return configuredStyle == BraceStyle.Allman
                ? isBraceOnDeclarationLine
                : !isBraceOnDeclarationLine;
        }

        private static string GetOpeningBracePlacementText(BraceStyle configuredStyle)
        {
            return configuredStyle == BraceStyle.Allman
                ? "on a new line"
                : "on the declaration line";
        }

        private static BraceCategory GetConfiguredCategories(SyntaxNodeAnalysisContext context)
        {
            return AnalyzerConfigCategoryParser.GetConfiguredBraceCategories(
                context,
                OptionKey);
        }

        private static BraceStyle GetConfiguredStyle(SyntaxNodeAnalysisContext context)
        {
            return AnalyzerConfigCategoryParser.GetConfiguredBraceStyle(
                context,
                StyleOptionKey);
        }
    }
}
