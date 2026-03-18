using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM1300Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1300";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Element must begin with an upper-case letter",
            messageFormat: "Element '{0}' must begin with an upper-case letter",
            category: "Naming",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The name of a C# element does not begin with an upper-case letter. Namespaces, classes, enums, structs, delegates, events, methods, and properties must begin with an upper-case letter.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(HandleNamespaceDeclaration, SyntaxKind.NamespaceDeclaration);
            context.RegisterSyntaxNodeAction(HandleNamespaceDeclaration, SyntaxKind.FileScopedNamespaceDeclaration);
            context.RegisterSyntaxNodeAction(HandleClassDeclaration, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(HandleEnumDeclaration, SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(HandleEnumMemberDeclaration, SyntaxKind.EnumMemberDeclaration);
            context.RegisterSyntaxNodeAction(HandleStructDeclaration, SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(HandleDelegateDeclaration, SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeAction(HandleEventDeclaration, SyntaxKind.EventDeclaration);
            context.RegisterSyntaxNodeAction(HandleEventFieldDeclaration, SyntaxKind.EventFieldDeclaration);
            context.RegisterSyntaxNodeAction(HandleMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(HandlePropertyDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private static void HandleNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            NameSyntax? name = context.Node switch
            {
                NamespaceDeclarationSyntax ns => ns.Name,
                FileScopedNamespaceDeclarationSyntax fsns => fsns.Name,
                _ => null,
            };

            if (name != null)
                CheckNamespaceName(context, name);
        }

        private static void CheckNamespaceName(SyntaxNodeAnalysisContext context, NameSyntax name)
        {
            if (name == null || name.IsMissing)
                return;

            if (name is QualifiedNameSyntax qualified)
            {
                CheckNamespaceName(context, qualified.Left);
                CheckNamespaceName(context, qualified.Right);
                return;
            }

            if (name is SimpleNameSyntax simple)
                CheckIdentifier(context, simple.Identifier);
        }

        private static void HandleClassDeclaration(SyntaxNodeAnalysisContext context)
            => CheckIdentifier(context, ((ClassDeclarationSyntax)context.Node).Identifier);

        private static void HandleEnumDeclaration(SyntaxNodeAnalysisContext context)
            => CheckIdentifier(context, ((EnumDeclarationSyntax)context.Node).Identifier);

        private static void HandleEnumMemberDeclaration(SyntaxNodeAnalysisContext context)
            => CheckIdentifier(context, ((EnumMemberDeclarationSyntax)context.Node).Identifier, allowUnderscoreDigit: true);

        private static void HandleStructDeclaration(SyntaxNodeAnalysisContext context)
            => CheckIdentifier(context, ((StructDeclarationSyntax)context.Node).Identifier);

        private static void HandleDelegateDeclaration(SyntaxNodeAnalysisContext context)
            => CheckIdentifier(context, ((DelegateDeclarationSyntax)context.Node).Identifier);

        private static void HandleEventDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (EventDeclarationSyntax)context.Node;
            if (node.Modifiers.Any(SyntaxKind.OverrideKeyword))
                return;
            CheckIdentifier(context, node.Identifier);
        }

        private static void HandleEventFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (EventFieldDeclarationSyntax)context.Node;
            if (node.Declaration == null || node.Declaration.IsMissing)
                return;
            foreach (var variable in node.Declaration.Variables)
            {
                if (variable == null || variable.IsMissing)
                    continue;
                CheckIdentifier(context, variable.Identifier);
            }
        }

        private static void HandleMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (MethodDeclarationSyntax)context.Node;
            if (node.Modifiers.Any(SyntaxKind.OverrideKeyword))
                return;
            CheckIdentifier(context, node.Identifier);
        }

        private static void HandlePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var node = (PropertyDeclarationSyntax)context.Node;
            if (node.Modifiers.Any(SyntaxKind.OverrideKeyword))
                return;
            CheckIdentifier(context, node.Identifier);
        }

        private static void CheckIdentifier(SyntaxNodeAnalysisContext context, SyntaxToken identifier, bool allowUnderscoreDigit = false)
        {
            if (identifier.IsMissing || string.IsNullOrEmpty(identifier.ValueText))
                return;

            char first = identifier.ValueText[0];
            if (!char.IsLower(first) && first != '_')
                return;

            if (allowUnderscoreDigit && identifier.ValueText.Length > 1 && first == '_' && char.IsDigit(identifier.ValueText[1]))
                return;

            var symbol = identifier.Parent != null ? context.SemanticModel.GetDeclaredSymbol(identifier.Parent) : null;
            if (symbol != null && IsImplementingInterfaceMember(symbol))
                return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.GetLocation(), identifier.ValueText));
        }

        private static bool IsImplementingInterfaceMember(ISymbol symbol)
        {
            switch (symbol)
            {
                case IMethodSymbol method:
                    return method.ExplicitInterfaceImplementations.Length > 0 || IsImplicitInterfaceImpl(symbol);
                case IPropertySymbol property:
                    return property.ExplicitInterfaceImplementations.Length > 0 || IsImplicitInterfaceImpl(symbol);
                case IEventSymbol evt:
                    return evt.ExplicitInterfaceImplementations.Length > 0 || IsImplicitInterfaceImpl(symbol);
                default:
                    return false;
            }
        }

        private static bool IsImplicitInterfaceImpl(ISymbol symbol)
        {
            var type = symbol.ContainingType;
            if (type == null)
                return false;

            foreach (var iface in type.AllInterfaces)
            {
                foreach (var member in iface.GetMembers())
                {
                    if (SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(member), symbol))
                        return true;
                }
            }

            return false;
        }
    }
}