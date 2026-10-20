using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0135Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0135";
        public const string ParameterNameKey = "ParameterName";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "All arguments must use named syntax when any argument is named",
            messageFormat: "Provide a name for this argument since another argument in this call uses named syntax",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a method or constructor call, if at least one argument uses named syntax (name: value), all arguments must use named syntax.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterOperationAction(
                AnalyzeOperation,
                OperationKind.Invocation,
                OperationKind.ObjectCreation);
        }

        private static void AnalyzeOperation(OperationAnalysisContext context)
        {
            ImmutableArray<IArgumentOperation> arguments;

            switch (context.Operation)
            {
                case IInvocationOperation invocation:
                    arguments = invocation.Arguments;
                    break;
                case IObjectCreationOperation objectCreation:
                    arguments = objectCreation.Arguments;
                    break;
                default:
                    return;
            }

            // Only the arguments written in the call's own list: an extension method's receiver
            // (builder.Header(name: ...)) is an argument too, but it can't take a name.
            var argumentList = (context.Operation.Syntax as InvocationExpressionSyntax)?.ArgumentList
                ?? (context.Operation.Syntax as BaseObjectCreationExpressionSyntax)?.ArgumentList;
            var explicitArgs = arguments
                .Where(a => a.ArgumentKind == ArgumentKind.Explicit)
                .Where(a => argumentList != null && argumentList.Span.Contains(a.Syntax.Span))
                .ToList();

            if (explicitArgs.Count == 0)
                return;

            bool anyNamed = explicitArgs.Any(IsNamedInSyntax);
            if (!anyNamed)
                return;

            bool anyUnnamed = explicitArgs.Any(a => !IsNamedInSyntax(a));
            if (!anyUnnamed)
                return;

            foreach (var arg in explicitArgs)
            {
                if (IsNamedInSyntax(arg))
                    continue;

                var paramName = arg.Parameter?.Name;
                var properties = ImmutableDictionary<string, string?>.Empty;
                if (paramName != null)
                    properties = properties.Add(ParameterNameKey, paramName);

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, arg.Syntax.GetLocation(), properties));
            }
        }

        private static bool IsNamedInSyntax(IArgumentOperation arg)
        {
            // A null-forgiving argument (name: x!) has the expression inside the '!' as its syntax, not the
            // ArgumentSyntax. An explicit argument's syntax always sits inside its own ArgumentSyntax.
            var argSyntax = arg.Syntax.FirstAncestorOrSelf<ArgumentSyntax>();
            return argSyntax?.NameColon != null;
        }
    }
}
