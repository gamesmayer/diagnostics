using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0114Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0114";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0114.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space between accessors in single-line property",
            messageFormat: "{0} space between accessors in single-line property",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether a space is required between adjacent accessors in a single-line property accessor list.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.AccessorList);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var accessorList = (AccessorListSyntax)context.Node;

            if (accessorList.Accessors.Count < 2)
                return;

            var lineSpan = accessorList.SyntaxTree.GetLineSpan(accessorList.Span);
            if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
                return;

            var enabled = GetEnabled(context);
            var sourceText = accessorList.SyntaxTree.GetText();

            for (var i = 0; i < accessorList.Accessors.Count - 1; i++)
            {
                var current = accessorList.Accessors[i];
                var next = accessorList.Accessors[i + 1];

                var betweenSpan = TextSpan.FromBounds(current.SemicolonToken.Span.End, next.SpanStart);
                var betweenText = sourceText.ToString(betweenSpan);

                if (betweenText.IndexOf('\n') >= 0 || betweenText.IndexOf('\r') >= 0)
                    continue;

                var hasSpace = betweenText.Length > 0;

                if (hasSpace == enabled)
                    continue;

                var action = enabled ? "Add" : "Remove";
                var properties = ImmutableDictionary<string, string?>.Empty
                    .Add(EnabledProperty, enabled.ToString());

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    current.SemicolonToken.GetLocation(),
                    properties,
                    action));
            }
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
