using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0036Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0036";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Variable declaration header must be on a single line",
            messageFormat: "Write the variable declaration header on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The variable declaration header (type, identifier, and assignment operator when present) must be on a single line. Assigned values may span multiple lines.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeLocalDeclarationStatement, SyntaxKind.LocalDeclarationStatement);
        }

        private static void AnalyzeLocalDeclarationStatement(SyntaxNodeAnalysisContext context)
        {
            var statement = (LocalDeclarationStatementSyntax)context.Node;
            var declaration = statement.Declaration;
            if (declaration == null || declaration.Variables.Count != 1)
            {
                return;
            }

            var variable = declaration.Variables[0];
            var tree = statement.SyntaxTree;
            var typeLine = tree.GetLineSpan(declaration.Type.GetLastToken().Span).EndLinePosition.Line;
            var identifierLine = tree.GetLineSpan(variable.Identifier.Span).StartLinePosition.Line;

            if (variable.Initializer == null)
            {
                var semicolonLine = tree.GetLineSpan(statement.SemicolonToken.Span).StartLinePosition.Line;
                if (typeLine != identifierLine || identifierLine != semicolonLine)
                {
                    ReportStatementDiagnostic(context, statement);
                }

                return;
            }

            var equalsLine = tree.GetLineSpan(variable.Initializer.EqualsToken.Span).StartLinePosition.Line;
            if (typeLine != identifierLine || identifierLine != equalsLine)
            {
                ReportStatementDiagnostic(context, statement);
            }
        }

        private static void ReportStatementDiagnostic(
            SyntaxNodeAnalysisContext context,
            LocalDeclarationStatementSyntax statement)
        {
            var declaration = statement.Declaration;
            var span = TextSpan.FromBounds(declaration.Type.SpanStart, statement.SemicolonToken.Span.End);
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(statement.SyntaxTree, span)));
        }
    }
}
