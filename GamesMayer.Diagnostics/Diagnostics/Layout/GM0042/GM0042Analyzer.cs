using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0042Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0042";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Blank line required after last using directive",
            messageFormat: "Add a blank line after the last using directive before '{0}'",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A blank line is required between the last using directive and the next declaration.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CompilationUnit);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = (CompilationUnitSyntax)context.Node;

            if (compilationUnit.Usings.Count == 0)
                return;

            if (compilationUnit.Members.Count == 0)
                return;

            var lastUsing = compilationUnit.Usings[compilationUnit.Usings.Count - 1];
            var firstMember = compilationUnit.Members[0];

            var usingEndLine = lastUsing.GetLocation().GetLineSpan().EndLinePosition.Line;
            var memberStartLine = firstMember.GetFirstToken().GetLocation().GetLineSpan().StartLinePosition.Line;

            if (memberStartLine - usingEndLine <= 1)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptor,
                        firstMember.GetFirstToken().GetLocation(),
                        GetMemberName(firstMember)));
            }
        }

        private static string GetMemberName(MemberDeclarationSyntax member) => member switch
        {
            NamespaceDeclarationSyntax ns => ns.Name.ToString(),
            FileScopedNamespaceDeclarationSyntax fsns => fsns.Name.ToString(),
            ClassDeclarationSyntax cls => cls.Identifier.Text,
            StructDeclarationSyntax str => str.Identifier.Text,
            InterfaceDeclarationSyntax iface => iface.Identifier.Text,
            EnumDeclarationSyntax enm => enm.Identifier.Text,
            DelegateDeclarationSyntax del => del.Identifier.Text,
            RecordDeclarationSyntax rec => rec.Identifier.Text,
            _ => "declaration"
        };
    }
}
