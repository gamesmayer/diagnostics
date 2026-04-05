using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using GamesMayer.Diagnostics.Utils;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0134Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0134";
        internal const string SidePropertyName = "Side";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank line allowed around colon in inheritance clause",
            messageFormat: "Remove blank line {0} ':' in inheritance clause",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In class and interface inheritance clauses, blank lines are not allowed before or after ':'.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBaseList, SyntaxKind.BaseList);
        }

        private static void AnalyzeBaseList(SyntaxNodeAnalysisContext context)
        {
            var baseList = (BaseListSyntax)context.Node;
            if (baseList.Parent is not ClassDeclarationSyntax and not InterfaceDeclarationSyntax)
            {
                return;
            }

            if (baseList.Types.Count == 0)
            {
                return;
            }

            var tree = baseList.SyntaxTree;
            var colonToken = baseList.ColonToken;
            var previousToken = colonToken.GetPreviousToken();
            if (previousToken == default)
            {
                return;
            }

            var firstBaseTypeToken = baseList.Types[0].GetFirstToken();
            if (firstBaseTypeToken == default)
            {
                return;
            }

            var sourceText = tree.GetText(context.CancellationToken);

            if (BlankLineDetectionUtils.TryGetFirstBlankLineStart(sourceText, tree, previousToken, colonToken, out var blankLineStart))
            {
                var gapLocation = Location.Create(tree, new TextSpan(blankLineStart, 0));
                var properties = ImmutableDictionary<string, string?>.Empty.Add(SidePropertyName, "before");
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    gapLocation,
                    properties,
                    "before"));
            }

            if (BlankLineDetectionUtils.TryGetFirstBlankLineStart(sourceText, tree, colonToken, firstBaseTypeToken, out blankLineStart))
            {
                var gapLocation = Location.Create(tree, new TextSpan(blankLineStart, 0));
                var properties = ImmutableDictionary<string, string?>.Empty.Add(SidePropertyName, "after");
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    gapLocation,
                    properties,
                    "after"));
            }
        }
    }
}