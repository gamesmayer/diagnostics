using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0131Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0131";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Anonymous object field must have explicit name",
            messageFormat: "Specify field name explicitly: '{0} = ...'",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "All fields in anonymous object creation expressions must use an explicit name assignment.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.AnonymousObjectMemberDeclarator);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var declarator = (AnonymousObjectMemberDeclaratorSyntax)context.Node;

            if (declarator.NameEquals != null)
                return;

            var inferredName = GetInferredName(declarator.Expression);
            if (inferredName == null)
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                declarator.Expression.GetLocation(),
                inferredName));
        }

        internal static string? GetInferredName(ExpressionSyntax expression)
        {
            return expression switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
                _ => null
            };
        }
    }
}
