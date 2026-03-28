using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0059Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0059";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Colon in named argument must be adjacent to parameter name",
            messageFormat: "Place ':' immediately after parameter name '{0}'",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When using named arguments in constructor or method calls, the ':' symbol must be written immediately after the parameter name with no whitespace before it.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeArgument, Microsoft.CodeAnalysis.CSharp.SyntaxKind.Argument);
        }

        private static void AnalyzeArgument(SyntaxNodeAnalysisContext context)
        {
            var argument = (ArgumentSyntax)context.Node;
            if (argument.NameColon is not { } nameColon)
                return;

            if (!IsSupportedArgumentList(argument.Parent?.Parent))
                return;

            var nameToken = nameColon.Name.Identifier;
            var colonToken = nameColon.ColonToken;

            if (nameToken == default || colonToken == default)
                return;

            if (nameToken.Span.End == colonToken.Span.Start)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, nameColon.GetLocation(), nameToken.ValueText));
        }

        private static bool IsSupportedArgumentList(SyntaxNode? parent)
        {
            return parent is InvocationExpressionSyntax
                or ObjectCreationExpressionSyntax
                or ImplicitObjectCreationExpressionSyntax
                or ConstructorInitializerSyntax;
        }
    }
}
