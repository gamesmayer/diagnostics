using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0108Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0108";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0108.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space after type in declarations",
            messageFormat: "{0} the space between declaration type and identifier",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether a space is required between declaration types and identifiers.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
            context.RegisterSyntaxNodeAction(AnalyzeTupleElement, SyntaxKind.TupleElement);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeLocalFunctionDeclaration, SyntaxKind.LocalFunctionStatement);
            context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeEventDeclaration, SyntaxKind.EventDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeDelegateDeclaration, SyntaxKind.DelegateDeclaration);
        }

        private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (VariableDeclarationSyntax)context.Node;
            if (declaration.Type == null || declaration.Variables.Count == 0)
                return;

            var identifier = declaration.Variables[0].Identifier;
            ReportIfMismatch(context, declaration.Type.GetLastToken(), identifier);
        }

        private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
        {
            var parameter = (ParameterSyntax)context.Node;
            if (parameter.Type == null)
                return;

            ReportIfMismatch(context, parameter.Type.GetLastToken(), parameter.Identifier);
        }

        private static void AnalyzeTupleElement(SyntaxNodeAnalysisContext context)
        {
            var tupleElement = (TupleElementSyntax)context.Node;
            if (tupleElement.Type == null || tupleElement.Identifier.IsKind(SyntaxKind.None))
                return;

            ReportIfMismatch(context, tupleElement.Type.GetLastToken(), tupleElement.Identifier);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;
            ReportIfMismatch(context, method.ReturnType.GetLastToken(), method.Identifier);
        }

        private static void AnalyzeLocalFunctionDeclaration(SyntaxNodeAnalysisContext context)
        {
            var localFunction = (LocalFunctionStatementSyntax)context.Node;
            ReportIfMismatch(context, localFunction.ReturnType.GetLastToken(), localFunction.Identifier);
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var property = (PropertyDeclarationSyntax)context.Node;
            ReportIfMismatch(context, property.Type.GetLastToken(), property.Identifier);
        }

        private static void AnalyzeEventDeclaration(SyntaxNodeAnalysisContext context)
        {
            var eventDeclaration = (EventDeclarationSyntax)context.Node;
            ReportIfMismatch(context, eventDeclaration.Type.GetLastToken(), eventDeclaration.Identifier);
        }

        private static void AnalyzeDelegateDeclaration(SyntaxNodeAnalysisContext context)
        {
            var delegateDeclaration = (DelegateDeclarationSyntax)context.Node;
            ReportIfMismatch(context, delegateDeclaration.ReturnType.GetLastToken(), delegateDeclaration.Identifier);
        }

        private static void ReportIfMismatch(
            SyntaxNodeAnalysisContext context,
            SyntaxToken typeLastToken,
            SyntaxToken identifier)
        {
            if (typeLastToken.IsKind(SyntaxKind.None) || identifier.IsKind(SyntaxKind.None))
                return;

            if (!TryHasSpaceBetweenTokens(context.Node.SyntaxTree, typeLastToken, identifier, out var hasSpace))
                return;

            var enabled = GetEnabled(context);
            if (hasSpace == enabled)
                return;

            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                identifier.GetLocation(),
                properties,
                action));
        }

        private static bool TryHasSpaceBetweenTokens(
            SyntaxTree tree,
            SyntaxToken leftToken,
            SyntaxToken rightToken,
            out bool hasSpace)
        {
            var betweenSpan = TextSpan.FromBounds(leftToken.Span.End, rightToken.SpanStart);
            var betweenText = tree.GetText().ToString(betweenSpan);

            if (betweenText.IndexOf('\n') >= 0 || betweenText.IndexOf('\r') >= 0)
            {
                hasSpace = false;
                return false;
            }

            foreach (var ch in betweenText)
            {
                if (!char.IsWhiteSpace(ch))
                {
                    hasSpace = false;
                    return false;
                }
            }

            hasSpace = betweenText.Length > 0;
            return true;
        }

        private static bool GetEnabled(SyntaxNodeAnalysisContext context)
        {
            var fileOptions = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (!fileOptions.TryGetValue(EnabledOptionKey, out var value) || string.IsNullOrWhiteSpace(value))
                return true;

            return bool.TryParse(value.Trim(), out var parsed) ? parsed : true;
        }
    }
}
