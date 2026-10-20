namespace GamesMayer.Diagnostics.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using Xunit;

    /// <summary>
    /// Runs every GM analyzer and fix-all, with the severities of templates/.editorconfig/.editorconfig.source, over
    /// code that used to make rules undo each other. Fixing must reach zero fixable diagnostics and then stop changing
    /// the code, like `just lint-fix` must on a real project.
    /// </summary>
    public class ConvergenceTests
    {
        private const int MaxPasses = 6;

        private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers = typeof(GM0001Analyzer).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
            .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type)!)
            .ToImmutableArray();

        private static readonly ImmutableArray<CodeFixProvider> CodeFixProviders = typeof(GM0001Analyzer).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(CodeFixProvider).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
            .Select(type => (CodeFixProvider)Activator.CreateInstance(type)!)
            .ToImmutableArray();

        public static IEnumerable<object[]> Samples => new[]
        {
            new object[] { "WrappedSingleArgument", @"namespace Sample
{
    public class Checks
    {
        public static void True(bool condition) { }

        public void Run(int a, int b, int c)
        {
            True(a == 1 || b == 2 || c == 3 || a == b);
        }
    }
}" },
            new object[] { "LambdaInIfCondition", @"namespace Sample
{
    using System;

    public class Users
    {
        public bool Any(Func<int, bool> predicate, int token) => true;

        public void Run(int userId)
        {
            if (Any(id => id == userId && id > 0, 1))
            {
                Run(userId + 1);
            }
        }
    }
}" },
            new object[] { "ForEachOverArray", @"namespace Sample
{
    public class Totals
    {
        public int Sum(int first, int second)
        {
            int total = 0;

            foreach (int value in new[] { first, second })
            {
                total += value;
            }

            return total;
        }
    }
}" },
            new object[] { "EmptyForSection", @"namespace Sample
{
    public class Pages
    {
        public int Count()
        {
            int pages = 0;

            for (int page = 1; ; page++)
            {
                pages = page;

                if (page > 3)
                {
                    break;
                }
            }

            return pages;
        }
    }
}" },
            new object[] { "NestedAnonymousObjects", @"namespace Sample
{
    public class Steam
    {
        public object Players(string id, string name)
        {
            return new { response = new { players = new[] { new { steamid = id, personaname = name } } } };
        }
    }
}" },
            new object[] { "SplitCallInsideShortList", @"namespace Sample
{
    public class Response
    {
        public int Code { get; set; }
    }

    public class Calls
    {
        public static void Equal(int expected, int actual) { }

        public static Response Operate(int a, int b, int c, int d) => new Response();

        public void Run()
        {
            Equal(200, Operate(1, 2, 3, 4).Code);
        }
    }
}" },
            new object[] { "UnorderedMembersAndUsings", @"namespace Sample
{
    using System.Text;
    using System.Collections.Generic;

    public class Store
    {
        public void Add(string value)
        {
            _items.Add(new StringBuilder(value).ToString());
        }

        private readonly List<string> _items = new List<string>();

        private static readonly int _limit = 10;

        public int Limit => _limit;
    }
}" },
            new object[] { "RecordWithoutBody", @"namespace Sample
{
    public record Identity(string Id, string Name);
}

namespace System.Runtime.CompilerServices
{
    internal class IsExternalInit
    {
    }
}" },
        };

        [Theory]
        [MemberData(nameof(Samples))]
        public async Task FixAll_ReachesZero_AndStops(string name, string source)
        {
            var document = CreateDocument(source);
            string? previous = null;

            for (var pass = 0; pass < MaxPasses; pass++)
            {
                document = await FixPassAsync(document);
                var text = (await document.GetTextAsync()).ToString();

                if (text == previous)
                    break;

                previous = text;
            }

            var remaining = await GetFixableDiagnosticsAsync(document);
            var finalText = (await document.GetTextAsync()).ToString();

            Assert.True(remaining.IsEmpty, $"{name}: {string.Join(", ", remaining.Select(d => d.Id + " " + d.Location.GetLineSpan().StartLinePosition))}\n{finalText}");
            Assert.Equal(finalText, (await (await FixPassAsync(document)).GetTextAsync()).ToString());
        }

        [Fact]
        public async Task Analyzers_ReportOnTheSamples()
        {
            var source = (string)Samples.First(sample => (string)sample[0] == "UnorderedMembersAndUsings")[1];
            var ids = (await GetFixableDiagnosticsAsync(CreateDocument(source))).Select(d => d.Id).ToList();

            Assert.Contains("GM1201", ids);
            Assert.Contains("GM1210", ids);
        }

        [Fact]
        public async Task FixAll_ChangesTheSamples()
        {
            var source = (string)Samples.First(sample => (string)sample[0] == "UnorderedMembersAndUsings")[1];
            var fixedDocument = await FixPassAsync(CreateDocument(source));

            Assert.NotEqual(source, (await fixedDocument.GetTextAsync()).ToString());
        }

        private static Document CreateDocument(string source)
        {
            var workspace = new AdhocWorkspace();
            var projectId = ProjectId.CreateNewId();
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
                .Split(Path.PathSeparator)
                .Where(path => Path.GetFileName(path).StartsWith("System.", StringComparison.Ordinal) || Path.GetFileName(path) == "netstandard.dll")
                .Select(path => MetadataReference.CreateFromFile(path));
            var documentId = DocumentId.CreateNewId(projectId);
            var solution = workspace.CurrentSolution
                .AddProject(projectId, "Sample", "Sample", LanguageNames.CSharp)
                .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithProjectParseOptions(projectId, new CSharpParseOptions(LanguageVersion.Preview))
                .AddMetadataReferences(projectId, references)
                .AddDocument(documentId, "Sample.cs", SourceText.From(source), filePath: "/src/Sample.cs")
                .AddAnalyzerConfigDocument(
                    DocumentId.CreateNewId(projectId),
                    ".editorconfig",
                    SourceText.From("root = true\n" + File.ReadAllText(FindTemplate())),
                    filePath: "/src/.editorconfig");

            return solution.GetDocument(documentId)!;
        }

        private static string FindTemplate()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "templates", ".editorconfig", ".editorconfig.source")))
                directory = directory.Parent;

            return Path.Combine(directory!.FullName, "templates", ".editorconfig", ".editorconfig.source");
        }

        // One `dotnet format analyzers` run: every diagnostic id fixed in turn with its fix-all.
        private static async Task<Document> FixPassAsync(Document document)
        {
            var ids = (await GetFixableDiagnosticsAsync(document)).Select(d => d.Id).Distinct().OrderBy(id => id).ToList();

            foreach (var id in ids)
            {
                var diagnostics = (await GetFixableDiagnosticsAsync(document)).Where(d => d.Id == id).ToImmutableArray();

                if (diagnostics.IsEmpty)
                    continue;

                var provider = CodeFixProviders.First(p => p.FixableDiagnosticIds.Contains(id));
                var context = new FixAllContext(
                    document,
                    provider,
                    FixAllScope.Document,
                    null,
                    new[] { id },
                    new DiagnosticProvider(diagnostics),
                    CancellationToken.None);
                var action = await provider.GetFixAllProvider()!.GetFixAsync(context);

                if (action == null)
                    continue;

                foreach (var operation in await action.GetOperationsAsync(CancellationToken.None))
                {
                    if (operation is ApplyChangesOperation apply)
                        document = apply.ChangedSolution.GetDocument(document.Id)!;
                }
            }

            return document;
        }

        private static async Task<ImmutableArray<Diagnostic>> GetFixableDiagnosticsAsync(Document document)
        {
            var compilation = (await document.Project.GetCompilationAsync())!;
            var diagnostics = await compilation
                .WithAnalyzers(Analyzers, document.Project.AnalyzerOptions)
                .GetAnalyzerDiagnosticsAsync();

            return diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Warning && d.Location.IsInSource)
                .Where(d => CodeFixProviders.Any(p => p.FixableDiagnosticIds.Contains(d.Id)))
                .ToImmutableArray();
        }

        private sealed class DiagnosticProvider : FixAllContext.DiagnosticProvider
        {
            private readonly ImmutableArray<Diagnostic> _diagnostics;

            public DiagnosticProvider(ImmutableArray<Diagnostic> diagnostics)
            {
                _diagnostics = diagnostics;
            }

            public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
                Task.FromResult<IEnumerable<Diagnostic>>(_diagnostics);

            public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken) =>
                Task.FromResult<IEnumerable<Diagnostic>>(_diagnostics);

            public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
                Task.FromResult<IEnumerable<Diagnostic>>(Enumerable.Empty<Diagnostic>());
        }
    }
}
