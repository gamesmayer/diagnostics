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
    public sealed class GM0121Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0121";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank line between constructor declaration and constructor initializer",
            messageFormat: "Remove the blank line between the constructor declaration and the constructor initializer",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Constructor declarations must not contain blank lines between the parameter list and the 'base' or 'this' initializer.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.BaseConstructorInitializer,
                SyntaxKind.ThisConstructorInitializer);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var initializer = (ConstructorInitializerSyntax)context.Node;
            if (initializer.Parent is not ConstructorDeclarationSyntax constructorDeclaration)
            {
                return;
            }

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            if (BlankLineDetectionUtils.TryGetFirstBlankLineStart(sourceText, tree, constructorDeclaration.ParameterList.CloseParenToken, initializer.ColonToken, out var blankLineStart))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, new TextSpan(blankLineStart, 0))));
            }

            if (BlankLineDetectionUtils.TryGetFirstBlankLineStart(sourceText, tree, initializer.ColonToken, initializer.ThisOrBaseKeyword, out blankLineStart))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, new TextSpan(blankLineStart, 0))));
            }
        }
    }
}
