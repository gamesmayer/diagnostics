using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0129Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0129";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0129.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Explicit object creation",
            messageFormat: "{0}",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether object creation must be explicit ('new Type(...)') or implicit ('new(...)') when simplification is possible.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.ImplicitObjectCreationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var enabled = GetEnabled(context);

            if (enabled)
            {
                if (context.Node is not ImplicitObjectCreationExpressionSyntax implicitCreation)
                    return;

                var properties = ImmutableDictionary<string, string?>.Empty
                    .Add(EnabledProperty, enabled.ToString());

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    implicitCreation.NewKeyword.GetLocation(),
                    properties,
                    "Use explicit object creation with the type name"));

                return;
            }

            if (context.Node is not ObjectCreationExpressionSyntax objectCreation)
                return;

            if (objectCreation.Type == null)
                return;

            if (!CanSimplifyToImplicitObjectCreation(context, objectCreation))
                return;

            var diagnosticProperties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                objectCreation.Type.GetLocation(),
                diagnosticProperties,
                "Simplify object creation to 'new(...)'"));
        }

        private static bool GetEnabled(SyntaxNodeAnalysisContext context)
        {
            var fileOptions = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (!fileOptions.TryGetValue(EnabledOptionKey, out var value) || string.IsNullOrWhiteSpace(value))
                return true;

            return bool.TryParse(value.Trim(), out var parsed) ? parsed : true;
        }

        private static bool CanSimplifyToImplicitObjectCreation(
            SyntaxNodeAnalysisContext context,
            ObjectCreationExpressionSyntax objectCreation)
        {
            var originalType = context.SemanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type;
            if (originalType == null)
                return false;

            // target-typed 'new()' cannot be used with 'var' declarations.
            if (objectCreation.Parent is EqualsValueClauseSyntax
                {
                    Parent: VariableDeclaratorSyntax
                    {
                        Parent: VariableDeclarationSyntax declaration
                    }
                }
                && declaration.Type.IsVar)
            {
                return false;
            }

            var operation = context.SemanticModel.GetOperation(objectCreation, context.CancellationToken) as IObjectCreationOperation;
            if (operation == null)
                return false;

            switch (operation.Parent)
            {
                case IVariableInitializerOperation variableInitializer
                    when variableInitializer.Parent is IVariableDeclaratorOperation variableDeclarator:
                    return SymbolEqualityComparer.Default.Equals(variableDeclarator.Symbol.Type, originalType);

                case IFieldInitializerOperation fieldInitializer:
                    foreach (var initializedField in fieldInitializer.InitializedFields)
                    {
                        if (SymbolEqualityComparer.Default.Equals(initializedField.Type, originalType))
                            return true;
                    }

                    return false;

                case ISimpleAssignmentOperation assignment:
                    return SymbolEqualityComparer.Default.Equals(assignment.Target.Type, originalType);

                case IArgumentOperation argument:
                    return SymbolEqualityComparer.Default.Equals(argument.Parameter?.Type, originalType);

                case IReturnOperation returnOperation:
                    return SymbolEqualityComparer.Default.Equals(returnOperation.ReturnedValue?.Type, originalType);

                default:
                    return false;
            }
        }

        internal static ImplicitObjectCreationExpressionSyntax BuildImplicitObjectCreationExpression(
            ObjectCreationExpressionSyntax objectCreation)
        {
            var argumentList = objectCreation.ArgumentList ?? SyntaxFactory.ArgumentList();
            return SyntaxFactory.ImplicitObjectCreationExpression(
                objectCreation.NewKeyword.WithTrailingTrivia(),
                argumentList,
                objectCreation.Initializer);
        }
    }
}
