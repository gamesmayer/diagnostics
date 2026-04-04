using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0130Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0130";
        private const string RulesOptionKey = "dotnet_diagnostic.GM0130.rules";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Namespace dependency violates clean architecture rules",
            messageFormat: "Namespace '{0}' cannot depend on '{1}'",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A namespace matching a protected pattern cannot have using directives that reference namespaces matching any configured disallowed pattern.");

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

            var rules = GetRules(context);
            if (rules.Count == 0)
                return;

            var namespaceDeclarations = compilationUnit
                .DescendantNodes()
                .OfType<BaseNamespaceDeclarationSyntax>()
                .ToList();

            if (namespaceDeclarations.Count == 0)
                return;

            // File-level usings apply to all namespaces in the file
            foreach (var usingDirective in compilationUnit.Usings)
            {
                var usingName = usingDirective.Name?.ToString();
                if (usingName == null)
                    continue;

                foreach (var ns in namespaceDeclarations)
                    CheckUsing(context, rules, ns.Name.ToString(), usingDirective, usingName);
            }

            // Namespace-level usings apply only to their own namespace
            foreach (var ns in namespaceDeclarations)
            {
                var namespaceName = ns.Name.ToString();
                foreach (var usingDirective in ns.Usings)
                {
                    var usingName = usingDirective.Name?.ToString();
                    if (usingName == null)
                        continue;

                    CheckUsing(context, rules, namespaceName, usingDirective, usingName);
                }
            }
        }

        private static void CheckUsing(
            SyntaxNodeAnalysisContext context,
            IReadOnlyList<DependencyRule> rules,
            string namespaceName,
            UsingDirectiveSyntax usingDirective,
            string usingName)
        {
            foreach (var rule in rules)
            {
                if (!MatchesPattern(namespaceName, rule.ProtectedPattern))
                    continue;

                foreach (var disallowedPattern in rule.DisallowedPatterns)
                {
                    if (MatchesPattern(usingName, disallowedPattern))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptor,
                            usingDirective.Name!.GetLocation(),
                            namespaceName,
                            usingName));
                        return;
                    }
                }
            }
        }

        private static IReadOnlyList<DependencyRule> GetRules(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (!options.TryGetValue(RulesOptionKey, out var value) || string.IsNullOrWhiteSpace(value))
                return Array.Empty<DependencyRule>();

            return ParseRules(value);
        }

        private static IReadOnlyList<DependencyRule> ParseRules(string value)
        {
            var rules = new List<DependencyRule>();

            foreach (var ruleString in value.Split('|'))
            {
                var parts = ruleString.Split(new[] { "->" }, 2, StringSplitOptions.None);
                if (parts.Length != 2)
                    continue;

                var protectedPattern = parts[0].Trim();
                var disallowedPatterns = parts[1]
                    .Split(',')
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToArray();

                if (!string.IsNullOrEmpty(protectedPattern) && disallowedPatterns.Length > 0)
                    rules.Add(new DependencyRule(protectedPattern, disallowedPatterns));
            }

            return rules;
        }

        private static bool MatchesPattern(string namespaceName, string pattern)
        {
            if (pattern.StartsWith("*.", StringComparison.Ordinal))
            {
                var suffix = pattern.Substring(1); // ".Domain"
                return namespaceName.EndsWith(suffix, StringComparison.Ordinal)
                    || namespaceName == pattern.Substring(2); // "Domain" (no prefix)
            }

            if (pattern.EndsWith(".*", StringComparison.Ordinal))
            {
                var prefix = pattern.Substring(0, pattern.Length - 2); // "MyApp"
                return namespaceName.StartsWith(prefix + ".", StringComparison.Ordinal)
                    || namespaceName == prefix;
            }

            return string.Equals(namespaceName, pattern, StringComparison.Ordinal);
        }

        private readonly struct DependencyRule
        {
            public DependencyRule(string protectedPattern, string[] disallowedPatterns)
            {
                ProtectedPattern = protectedPattern;
                DisallowedPatterns = disallowedPatterns;
            }

            public string ProtectedPattern { get; }
            public string[] DisallowedPatterns { get; }
        }
    }
}
