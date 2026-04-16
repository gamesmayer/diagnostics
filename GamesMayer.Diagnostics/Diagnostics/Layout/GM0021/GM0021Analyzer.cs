using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using GamesMayer.Diagnostics.Utils;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0021Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0021";
        private const string OptionKey = "dotnet_diagnostic.GM0021";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Opening brace required on declaration line for empty enclosure",
            messageFormat: "Place the opening brace on the declaration line for empty enclosures",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: false,
            description: "Opening braces for empty enclosures must remain on the declaration line.");

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

            switch (context.Node)
            {
                case NamespaceDeclarationSyntax namespaceDeclaration:
                    AnalyzeBrace(context, configuredCategories, BraceCategory.Types, namespaceDeclaration.OpenBraceToken, IsEffectivelyEmpty(namespaceDeclaration.OpenBraceToken, namespaceDeclaration.CloseBraceToken, namespaceDeclaration.Members.Count));
                    break;
                case TypeDeclarationSyntax typeDeclaration:
                    AnalyzeBrace(context, configuredCategories, BraceCategory.Types, typeDeclaration.OpenBraceToken, IsEffectivelyEmpty(typeDeclaration.OpenBraceToken, typeDeclaration.CloseBraceToken, typeDeclaration.Members.Count));
                    break;
                case EnumDeclarationSyntax enumDeclaration:
                    AnalyzeBrace(context, configuredCategories, BraceCategory.Types, enumDeclaration.OpenBraceToken, IsEffectivelyEmpty(enumDeclaration.OpenBraceToken, enumDeclaration.CloseBraceToken, enumDeclaration.Members.Count));
                    break;
                case PropertyDeclarationSyntax propertyDeclaration:
                    if (propertyDeclaration.AccessorList != null)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.Properties, propertyDeclaration.AccessorList.OpenBraceToken, IsEffectivelyEmpty(propertyDeclaration.AccessorList.OpenBraceToken, propertyDeclaration.AccessorList.CloseBraceToken, propertyDeclaration.AccessorList.Accessors.Count));
                    }

                    break;
                case IndexerDeclarationSyntax indexerDeclaration:
                    if (indexerDeclaration.AccessorList != null)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.Indexers, indexerDeclaration.AccessorList.OpenBraceToken, IsEffectivelyEmpty(indexerDeclaration.AccessorList.OpenBraceToken, indexerDeclaration.AccessorList.CloseBraceToken, indexerDeclaration.AccessorList.Accessors.Count));
                    }

                    break;
                case EventDeclarationSyntax eventDeclaration:
                    if (eventDeclaration.AccessorList != null)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.Events, eventDeclaration.AccessorList.OpenBraceToken, IsEffectivelyEmpty(eventDeclaration.AccessorList.OpenBraceToken, eventDeclaration.AccessorList.CloseBraceToken, eventDeclaration.AccessorList.Accessors.Count));
                    }

                    break;
                case AccessorDeclarationSyntax accessorDeclaration:
                    if (accessorDeclaration.Body != null)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.Accessors, accessorDeclaration.Body.OpenBraceToken, IsEffectivelyEmpty(accessorDeclaration.Body));
                    }

                    break;
                case MethodDeclarationSyntax methodDeclaration:
                    if (methodDeclaration.Body != null)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.Methods, methodDeclaration.Body.OpenBraceToken, IsEffectivelyEmpty(methodDeclaration.Body));
                    }

                    break;
                case ConstructorDeclarationSyntax constructorDeclaration:
                    if (constructorDeclaration.Body != null)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.Methods, constructorDeclaration.Body.OpenBraceToken, IsEffectivelyEmpty(constructorDeclaration.Body));
                    }

                    break;
                case DestructorDeclarationSyntax destructorDeclaration:
                    if (destructorDeclaration.Body != null)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.Methods, destructorDeclaration.Body.OpenBraceToken, IsEffectivelyEmpty(destructorDeclaration.Body));
                    }

                    break;
                case OperatorDeclarationSyntax operatorDeclaration:
                    if (operatorDeclaration.Body != null)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.Methods, operatorDeclaration.Body.OpenBraceToken, IsEffectivelyEmpty(operatorDeclaration.Body));
                    }

                    break;
                case ConversionOperatorDeclarationSyntax conversionOperatorDeclaration:
                    if (conversionOperatorDeclaration.Body != null)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.Methods, conversionOperatorDeclaration.Body.OpenBraceToken, IsEffectivelyEmpty(conversionOperatorDeclaration.Body));
                    }

                    break;
                case LocalFunctionStatementSyntax localFunctionStatement:
                    if (localFunctionStatement.Body != null)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.LocalFunctions, localFunctionStatement.Body.OpenBraceToken, IsEffectivelyEmpty(localFunctionStatement.Body));
                    }

                    break;
                case AnonymousMethodExpressionSyntax anonymousMethodExpression:
                    if (anonymousMethodExpression.Block != null)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.AnonymousMethods, anonymousMethodExpression.Block.OpenBraceToken, IsEffectivelyEmpty(anonymousMethodExpression.Block));
                    }

                    break;
                case ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpression:
                    if (parenthesizedLambdaExpression.Body is BlockSyntax parenthesizedLambdaBody)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.Lambdas, parenthesizedLambdaBody.OpenBraceToken, IsEffectivelyEmpty(parenthesizedLambdaBody));
                    }

                    break;
                case SimpleLambdaExpressionSyntax simpleLambdaExpression:
                    if (simpleLambdaExpression.Body is BlockSyntax simpleLambdaBody)
                    {
                        AnalyzeBrace(context, configuredCategories, BraceCategory.Lambdas, simpleLambdaBody.OpenBraceToken, IsEffectivelyEmpty(simpleLambdaBody));
                    }

                    break;
                case IfStatementSyntax ifStatement:
                    AnalyzeControlBlock(context, configuredCategories, ifStatement.Statement);
                    break;
                case ElseClauseSyntax elseClause:
                    AnalyzeControlBlock(context, configuredCategories, elseClause.Statement);
                    break;
                case ForStatementSyntax forStatement:
                    AnalyzeControlBlock(context, configuredCategories, forStatement.Statement);
                    break;
                case ForEachStatementSyntax forEachStatement:
                    AnalyzeControlBlock(context, configuredCategories, forEachStatement.Statement);
                    break;
                case ForEachVariableStatementSyntax forEachVariableStatement:
                    AnalyzeControlBlock(context, configuredCategories, forEachVariableStatement.Statement);
                    break;
                case WhileStatementSyntax whileStatement:
                    AnalyzeControlBlock(context, configuredCategories, whileStatement.Statement);
                    break;
                case DoStatementSyntax doStatement:
                    AnalyzeControlBlock(context, configuredCategories, doStatement.Statement);
                    break;
                case UsingStatementSyntax usingStatement:
                    AnalyzeControlBlock(context, configuredCategories, usingStatement.Statement);
                    break;
                case LockStatementSyntax lockStatement:
                    AnalyzeControlBlock(context, configuredCategories, lockStatement.Statement);
                    break;
                case FixedStatementSyntax fixedStatement:
                    AnalyzeControlBlock(context, configuredCategories, fixedStatement.Statement);
                    break;
                case CheckedStatementSyntax checkedStatement:
                    AnalyzeBrace(context, configuredCategories, BraceCategory.ControlBlocks, checkedStatement.Block.OpenBraceToken, IsEffectivelyEmpty(checkedStatement.Block));
                    break;
                case UnsafeStatementSyntax unsafeStatement:
                    AnalyzeBrace(context, configuredCategories, BraceCategory.ControlBlocks, unsafeStatement.Block.OpenBraceToken, IsEffectivelyEmpty(unsafeStatement.Block));
                    break;
                case SwitchStatementSyntax switchStatement:
                    AnalyzeBrace(context, configuredCategories, BraceCategory.ControlBlocks, switchStatement.OpenBraceToken, IsEffectivelyEmpty(switchStatement.OpenBraceToken, switchStatement.CloseBraceToken, switchStatement.Sections.Count));
                    break;
                case TryStatementSyntax tryStatement:
                    AnalyzeBrace(context, configuredCategories, BraceCategory.ControlBlocks, tryStatement.Block.OpenBraceToken, IsEffectivelyEmpty(tryStatement.Block));
                    break;
                case CatchClauseSyntax catchClause:
                    AnalyzeBrace(context, configuredCategories, BraceCategory.ControlBlocks, catchClause.Block.OpenBraceToken, IsEffectivelyEmpty(catchClause.Block));
                    break;
                case FinallyClauseSyntax finallyClause:
                    AnalyzeBrace(context, configuredCategories, BraceCategory.ControlBlocks, finallyClause.Block.OpenBraceToken, IsEffectivelyEmpty(finallyClause.Block));
                    break;
                case InitializerExpressionSyntax initializerExpression:
                    AnalyzeBrace(context, configuredCategories, BraceCategory.ObjectCollectionArrayInitializers, initializerExpression.OpenBraceToken, IsEffectivelyEmpty(initializerExpression.OpenBraceToken, initializerExpression.CloseBraceToken, initializerExpression.Expressions.Count));
                    break;
                case AnonymousObjectCreationExpressionSyntax anonymousObjectCreation:
                    AnalyzeBrace(context, configuredCategories, BraceCategory.AnonymousTypes, anonymousObjectCreation.OpenBraceToken, IsEffectivelyEmpty(anonymousObjectCreation.OpenBraceToken, anonymousObjectCreation.CloseBraceToken, anonymousObjectCreation.Initializers.Count));
                    break;
            }
        }

        private static void AnalyzeControlBlock(
            SyntaxNodeAnalysisContext context,
            BraceCategory configuredCategories,
            StatementSyntax statement)
        {
            if (statement is BlockSyntax block)
            {
                AnalyzeBrace(context, configuredCategories, BraceCategory.ControlBlocks, block.OpenBraceToken, IsEffectivelyEmpty(block));
            }
        }

        private static void AnalyzeBrace(
            SyntaxNodeAnalysisContext context,
            BraceCategory configuredCategories,
            BraceCategory category,
            SyntaxToken openBrace,
            bool isEmptyEnclosure)
        {
            if (openBrace.IsMissing || !isEmptyEnclosure || (configuredCategories & category) == 0)
            {
                return;
            }

            if (openBrace.Parent?.ContainsDirectives == true)
            {
                return;
            }

            var previousToken = openBrace.GetPreviousToken(includeZeroWidth: false);
            if (previousToken == default)
            {
                return;
            }

            var tree = context.Node.SyntaxTree;
            var previousTokenLine = tree.GetLineSpan(previousToken.Span).EndLinePosition.Line;
            var openBraceLine = tree.GetLineSpan(openBrace.Span).StartLinePosition.Line;

            if (previousTokenLine != openBraceLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, openBrace.GetLocation()));
            }
        }

        private static bool IsEffectivelyEmpty(BlockSyntax block)
        {
            return IsEffectivelyEmpty(block.OpenBraceToken, block.CloseBraceToken, block.Statements.Count);
        }

        private static bool IsEffectivelyEmpty(SyntaxToken openBrace, SyntaxToken closeBrace, int count)
        {
            if (count != 0)
            {
                return false;
            }

            return !CommentUtils.HasCommentTrivia(openBrace.TrailingTrivia) && !CommentUtils.HasCommentTrivia(closeBrace.LeadingTrivia);
        }

        private static BraceCategory GetConfiguredCategories(SyntaxNodeAnalysisContext context)
        {
            return AnalyzerConfigCategoryParser.GetConfiguredBraceCategories(
                context,
                OptionKey);
        }
    }
}
