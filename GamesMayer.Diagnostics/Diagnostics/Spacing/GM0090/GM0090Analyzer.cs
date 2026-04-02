using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0090Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0090";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0090.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space in empty method declaration parameter list",
            messageFormat: "{0} the space in empty method declaration parameter list",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether a space is required between parentheses of an empty method declaration parameter list. Equivalent to the csharp_space_between_method_declaration_empty_parameter_list_parentheses EditorConfig option.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!TryGetEmptyParameterList(context.Node, out var openParen, out var closeParen))
                return;

            var hasSpace = HasSpaceBetweenParens(context.Node.SyntaxTree, openParen, closeParen);
            var enabled = GetEnabled(context);

            if (hasSpace == enabled)
                return;

            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                openParen.GetLocation(),
                properties,
                action));
        }

        private static bool TryGetEmptyParameterList(SyntaxNode node, out SyntaxToken openParen, out SyntaxToken closeParen)
        {
            ParameterListSyntax? parameterList = node switch
            {
                MethodDeclarationSyntax method => method.ParameterList,
                ConstructorDeclarationSyntax ctor => ctor.ParameterList,
                DestructorDeclarationSyntax dtor => dtor.ParameterList,
                _ => null
            };

            if (parameterList == null || parameterList.Parameters.Count > 0)
            {
                openParen = default;
                closeParen = default;
                return false;
            }

            openParen = parameterList.OpenParenToken;
            closeParen = parameterList.CloseParenToken;
            return true;
        }

        private static bool HasSpaceBetweenParens(SyntaxTree tree, SyntaxToken openParen, SyntaxToken closeParen)
        {
            var betweenSpan = TextSpan.FromBounds(openParen.Span.End, closeParen.SpanStart);
            var betweenText = tree.GetText().ToString(betweenSpan);

            foreach (var ch in betweenText)
            {
                if (!char.IsWhiteSpace(ch))
                    return false;
            }

            return betweenText.Length > 0;
        }

        private static bool GetEnabled(SyntaxNodeAnalysisContext context)
        {
            var fileOptions = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (!fileOptions.TryGetValue(EnabledOptionKey, out var value) || string.IsNullOrWhiteSpace(value))
                return false;

            return bool.TryParse(value.Trim(), out var parsed) && parsed;
        }
    }
}
