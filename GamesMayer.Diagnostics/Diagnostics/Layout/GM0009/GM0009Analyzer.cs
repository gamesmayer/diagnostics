using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0009Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0009";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No line breaks between modifiers, type, and identifier",
            messageFormat: "'{0}' declaration must not have line breaks between modifiers, type, and identifier",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Class declarations and class member declarations must not have line breaks between modifiers, type, and identifier.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeStructDeclaration, SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeRecordDeclaration, SyntaxKind.RecordDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeEventDeclaration, SyntaxKind.EventDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;
            AnalyzeTypeDeclaration(context, classDecl.Modifiers, classDecl.Keyword, classDecl.Identifier, "class");
        }

        private static void AnalyzeStructDeclaration(SyntaxNodeAnalysisContext context)
        {
            var structDecl = (StructDeclarationSyntax)context.Node;
            AnalyzeTypeDeclaration(context, structDecl.Modifiers, structDecl.Keyword, structDecl.Identifier, "struct");
        }

        private static void AnalyzeRecordDeclaration(SyntaxNodeAnalysisContext context)
        {
            var recordDecl = (RecordDeclarationSyntax)context.Node;
            AnalyzeTypeDeclaration(context, recordDecl.Modifiers, recordDecl.Keyword, recordDecl.Identifier, "record");
        }

        private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var interfaceDecl = (InterfaceDeclarationSyntax)context.Node;
            AnalyzeTypeDeclaration(context, interfaceDecl.Modifiers, interfaceDecl.Keyword, interfaceDecl.Identifier, "interface");
        }

        private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var fieldDecl = (FieldDeclarationSyntax)context.Node;
            var firstToken = GetFirstNonAttributeToken(fieldDecl.AttributeLists, fieldDecl.GetFirstToken());
            var lastToken = fieldDecl.Declaration.Variables.FirstOrDefault()?.Identifier ?? fieldDecl.GetLastToken();
            
            if (HasLineBreaksBetween(firstToken, lastToken))
            {
                var fieldName = fieldDecl.Declaration.Variables.FirstOrDefault()?.Identifier.Text ?? "field";
                var location = Location.Create(fieldDecl.SyntaxTree, Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(firstToken.SpanStart, lastToken.Span.End));
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, fieldName));
            }
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var propertyDecl = (PropertyDeclarationSyntax)context.Node;
            var firstToken = GetFirstNonAttributeToken(propertyDecl.AttributeLists, propertyDecl.GetFirstToken());
            
            if (HasLineBreaksBetween(firstToken, propertyDecl.Identifier))
            {
                var location = Location.Create(propertyDecl.SyntaxTree, Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(firstToken.SpanStart, propertyDecl.Identifier.Span.End));
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, propertyDecl.Identifier.Text));
            }
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDecl = (MethodDeclarationSyntax)context.Node;
            var firstToken = GetFirstNonAttributeToken(methodDecl.AttributeLists, methodDecl.GetFirstToken());
            
            if (HasLineBreaksBetween(firstToken, methodDecl.Identifier))
            {
                var location = Location.Create(methodDecl.SyntaxTree, Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(firstToken.SpanStart, methodDecl.Identifier.Span.End));
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, methodDecl.Identifier.Text));
            }
        }

        private static void AnalyzeEventDeclaration(SyntaxNodeAnalysisContext context)
        {
            var eventDecl = (EventDeclarationSyntax)context.Node;
            var firstToken = GetFirstNonAttributeToken(eventDecl.AttributeLists, eventDecl.GetFirstToken());
            
            if (HasLineBreaksBetween(firstToken, eventDecl.Identifier))
            {
                var location = Location.Create(eventDecl.SyntaxTree, Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(firstToken.SpanStart, eventDecl.Identifier.Span.End));
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, eventDecl.Identifier.Text));
            }
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var constructorDecl = (ConstructorDeclarationSyntax)context.Node;
            var firstToken = GetFirstNonAttributeToken(constructorDecl.AttributeLists, constructorDecl.GetFirstToken());
            
            if (HasLineBreaksBetween(firstToken, constructorDecl.Identifier))
            {
                var location = Location.Create(constructorDecl.SyntaxTree, Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(firstToken.SpanStart, constructorDecl.Identifier.Span.End));
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, constructorDecl.Identifier.Text));
            }
        }

        private static void AnalyzeTypeDeclaration(
            SyntaxNodeAnalysisContext context,
            SyntaxTokenList modifiers,
            SyntaxToken keyword,
            SyntaxToken identifier,
            string typeName)
        {
            var firstToken = modifiers.Count > 0 ? modifiers[0] : keyword;
            
            if (HasLineBreaksBetween(firstToken, identifier))
            {
                var location = Location.Create(context.Node.SyntaxTree, Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(firstToken.SpanStart, identifier.Span.End));
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, identifier.Text));
            }
        }

        private static SyntaxToken GetFirstNonAttributeToken(SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken defaultToken)
        {
            return attributeLists.Count > 0
                ? attributeLists.Last().GetLastToken().GetNextToken()
                : defaultToken;
        }

        private static bool HasLineBreaksBetween(SyntaxToken startToken, SyntaxToken endToken)
        {
            var startLine = startToken.GetLocation().GetLineSpan().StartLinePosition.Line;
            var endLine = endToken.GetLocation().GetLineSpan().StartLinePosition.Line;
            return startLine != endLine;
        }
    }
}
