using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0019Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0019";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Empty braces enclosure must be on a single line",
            messageFormat: "Write empty braces on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Empty enclosure braces must be written on a single line with a single space between them.");

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
            switch (context.Node)
            {
                case NamespaceDeclarationSyntax ns:
                    CheckBraces(context, ns.OpenBraceToken, ns.CloseBraceToken, ns.Members.Count == 0);
                    break;
                case TypeDeclarationSyntax type:
                    CheckBraces(context, type.OpenBraceToken, type.CloseBraceToken, type.Members.Count == 0);
                    break;
                case EnumDeclarationSyntax enumDecl:
                    CheckBraces(context, enumDecl.OpenBraceToken, enumDecl.CloseBraceToken, enumDecl.Members.Count == 0);
                    break;
                case PropertyDeclarationSyntax property when property.AccessorList != null:
                    CheckBraces(context, property.AccessorList.OpenBraceToken, property.AccessorList.CloseBraceToken, property.AccessorList.Accessors.Count == 0);
                    break;
                case IndexerDeclarationSyntax indexer when indexer.AccessorList != null:
                    CheckBraces(context, indexer.AccessorList.OpenBraceToken, indexer.AccessorList.CloseBraceToken, indexer.AccessorList.Accessors.Count == 0);
                    break;
                case EventDeclarationSyntax eventDecl when eventDecl.AccessorList != null:
                    CheckBraces(context, eventDecl.AccessorList.OpenBraceToken, eventDecl.AccessorList.CloseBraceToken, eventDecl.AccessorList.Accessors.Count == 0);
                    break;
                case AccessorDeclarationSyntax accessor when accessor.Body != null:
                    CheckBraces(context, accessor.Body.OpenBraceToken, accessor.Body.CloseBraceToken, accessor.Body.Statements.Count == 0);
                    break;
                case MethodDeclarationSyntax method when method.Body != null:
                    CheckBraces(context, method.Body.OpenBraceToken, method.Body.CloseBraceToken, method.Body.Statements.Count == 0);
                    break;
                case ConstructorDeclarationSyntax ctor when ctor.Body != null:
                    CheckBraces(context, ctor.Body.OpenBraceToken, ctor.Body.CloseBraceToken, ctor.Body.Statements.Count == 0);
                    break;
                case DestructorDeclarationSyntax dtor when dtor.Body != null:
                    CheckBraces(context, dtor.Body.OpenBraceToken, dtor.Body.CloseBraceToken, dtor.Body.Statements.Count == 0);
                    break;
                case OperatorDeclarationSyntax op when op.Body != null:
                    CheckBraces(context, op.Body.OpenBraceToken, op.Body.CloseBraceToken, op.Body.Statements.Count == 0);
                    break;
                case ConversionOperatorDeclarationSyntax conv when conv.Body != null:
                    CheckBraces(context, conv.Body.OpenBraceToken, conv.Body.CloseBraceToken, conv.Body.Statements.Count == 0);
                    break;
                case LocalFunctionStatementSyntax local when local.Body != null:
                    CheckBraces(context, local.Body.OpenBraceToken, local.Body.CloseBraceToken, local.Body.Statements.Count == 0);
                    break;
                case AnonymousMethodExpressionSyntax anon when anon.Block != null:
                    CheckBraces(context, anon.Block.OpenBraceToken, anon.Block.CloseBraceToken, anon.Block.Statements.Count == 0);
                    break;
                case ParenthesizedLambdaExpressionSyntax lambda when lambda.Body is BlockSyntax lambdaBlock:
                    CheckBraces(context, lambdaBlock.OpenBraceToken, lambdaBlock.CloseBraceToken, lambdaBlock.Statements.Count == 0);
                    break;
                case SimpleLambdaExpressionSyntax simpleLambda when simpleLambda.Body is BlockSyntax simpleLambdaBlock:
                    CheckBraces(context, simpleLambdaBlock.OpenBraceToken, simpleLambdaBlock.CloseBraceToken, simpleLambdaBlock.Statements.Count == 0);
                    break;
                case IfStatementSyntax ifStmt when ifStmt.Statement is BlockSyntax ifBlock:
                    CheckBraces(context, ifBlock.OpenBraceToken, ifBlock.CloseBraceToken, ifBlock.Statements.Count == 0);
                    break;
                case ElseClauseSyntax elseClause when elseClause.Statement is BlockSyntax elseBlock:
                    CheckBraces(context, elseBlock.OpenBraceToken, elseBlock.CloseBraceToken, elseBlock.Statements.Count == 0);
                    break;
                case ForStatementSyntax forStmt when forStmt.Statement is BlockSyntax forBlock:
                    CheckBraces(context, forBlock.OpenBraceToken, forBlock.CloseBraceToken, forBlock.Statements.Count == 0);
                    break;
                case ForEachStatementSyntax forEach when forEach.Statement is BlockSyntax forEachBlock:
                    CheckBraces(context, forEachBlock.OpenBraceToken, forEachBlock.CloseBraceToken, forEachBlock.Statements.Count == 0);
                    break;
                case ForEachVariableStatementSyntax forEachVar when forEachVar.Statement is BlockSyntax forEachVarBlock:
                    CheckBraces(context, forEachVarBlock.OpenBraceToken, forEachVarBlock.CloseBraceToken, forEachVarBlock.Statements.Count == 0);
                    break;
                case WhileStatementSyntax whileStmt when whileStmt.Statement is BlockSyntax whileBlock:
                    CheckBraces(context, whileBlock.OpenBraceToken, whileBlock.CloseBraceToken, whileBlock.Statements.Count == 0);
                    break;
                case DoStatementSyntax doStmt when doStmt.Statement is BlockSyntax doBlock:
                    CheckBraces(context, doBlock.OpenBraceToken, doBlock.CloseBraceToken, doBlock.Statements.Count == 0);
                    break;
                case UsingStatementSyntax usingStmt when usingStmt.Statement is BlockSyntax usingBlock:
                    CheckBraces(context, usingBlock.OpenBraceToken, usingBlock.CloseBraceToken, usingBlock.Statements.Count == 0);
                    break;
                case LockStatementSyntax lockStmt when lockStmt.Statement is BlockSyntax lockBlock:
                    CheckBraces(context, lockBlock.OpenBraceToken, lockBlock.CloseBraceToken, lockBlock.Statements.Count == 0);
                    break;
                case FixedStatementSyntax fixedStmt when fixedStmt.Statement is BlockSyntax fixedBlock:
                    CheckBraces(context, fixedBlock.OpenBraceToken, fixedBlock.CloseBraceToken, fixedBlock.Statements.Count == 0);
                    break;
                case CheckedStatementSyntax checkedStmt:
                    CheckBraces(context, checkedStmt.Block.OpenBraceToken, checkedStmt.Block.CloseBraceToken, checkedStmt.Block.Statements.Count == 0);
                    break;
                case UnsafeStatementSyntax unsafeStmt:
                    CheckBraces(context, unsafeStmt.Block.OpenBraceToken, unsafeStmt.Block.CloseBraceToken, unsafeStmt.Block.Statements.Count == 0);
                    break;
                case SwitchStatementSyntax switchStmt:
                    CheckBraces(context, switchStmt.OpenBraceToken, switchStmt.CloseBraceToken, switchStmt.Sections.Count == 0);
                    break;
                case TryStatementSyntax tryStmt:
                    CheckBraces(context, tryStmt.Block.OpenBraceToken, tryStmt.Block.CloseBraceToken, tryStmt.Block.Statements.Count == 0);
                    break;
                case CatchClauseSyntax catchClause:
                    CheckBraces(context, catchClause.Block.OpenBraceToken, catchClause.Block.CloseBraceToken, catchClause.Block.Statements.Count == 0);
                    break;
                case FinallyClauseSyntax finallyClause:
                    CheckBraces(context, finallyClause.Block.OpenBraceToken, finallyClause.Block.CloseBraceToken, finallyClause.Block.Statements.Count == 0);
                    break;
                case InitializerExpressionSyntax initializer:
                    CheckBraces(context, initializer.OpenBraceToken, initializer.CloseBraceToken, initializer.Expressions.Count == 0);
                    break;
                case AnonymousObjectCreationExpressionSyntax anonObj:
                    CheckBraces(context, anonObj.OpenBraceToken, anonObj.CloseBraceToken, anonObj.Initializers.Count == 0);
                    break;
            }
        }

        private static void CheckBraces(
            SyntaxNodeAnalysisContext context,
            SyntaxToken openBraceToken,
            SyntaxToken closeBraceToken,
            bool isEmptyEnclosure)
        {
            if (!isEmptyEnclosure)
            {
                return;
            }

            ReportIfBraceEnclosureInvalid(context, openBraceToken, closeBraceToken);
        }

        private static void ReportIfBraceEnclosureInvalid(
            SyntaxNodeAnalysisContext context,
            SyntaxToken openBraceToken,
            SyntaxToken closeBraceToken)
        {
            if (openBraceToken.IsMissing || closeBraceToken.IsMissing)
            {
                return;
            }

            var tree = context.Node.SyntaxTree;
            var openBraceLine = tree.GetLineSpan(openBraceToken.Span).StartLinePosition.Line;
            var closeBraceLine = tree.GetLineSpan(closeBraceToken.Span).StartLinePosition.Line;
            var textBetweenBraces = tree.GetText(context.CancellationToken).ToString(TextSpan.FromBounds(openBraceToken.Span.End, closeBraceToken.Span.Start));

            if (openBraceLine == closeBraceLine && textBetweenBraces == " ")
            {
                return;
            }

            var span = TextSpan.FromBounds(openBraceToken.SpanStart, closeBraceToken.Span.End);
            var location = Location.Create(tree, span);
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
        }
    }
}
