namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0035Analyzer>;

    public class GM0035AnalyzerTests
    {
        [Fact]
        public async Task FluentChainWithoutBlankLines_NoDiagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
            .Where(x => x > 1)
            .Select(x => x * 2)
            .ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBetweenFluentSegments_Diagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
            .Where(x => x > 1)
{|GM0035:
|}            .Select(x => x * 2)
            .ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBeforeFirstFluentSegment_Diagnostic()
        {
            var testCode = @"class C
{
    CharacterDefinitionRepository characterDefinitionRepository;

    void M(string key)
    {
        var definition = characterDefinitionRepository
{|GM0035:
|}            .Find(key);
    }
}

class CharacterDefinitionRepository
{
    public object Find(string key) => new object();
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenFluentSegments_MultipleDiagnostics()
        {
            var testCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
            .Where(x => x > 1)
{|GM0035:
|}{|GM0035:
|}            .Select(x => x * 2)
            .ToList();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineInsideMultilineArgumentList_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = Configure(
            1,

            2);
    }

    C Configure(int a, int b)
    {
        return this;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleInvocation_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        DoWork();
    }

    void DoWork() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
