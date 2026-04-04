namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0131Analyzer>;

    public class GM0131AnalyzerTests
    {
        [Fact]
        public async Task AllFieldsExplicit_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(int x)
    {
        var result = new
        {
            value = x,
            doubled = x * 2
        };
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitMemberAccess_Diagnostic()
        {
            var testCode = @"class W { public string key; public string mapPrefab; }
class C
{
    void M(W w)
    {
        var result = new
        {
            prefab = w.mapPrefab,
            {|GM0131:w.key|}
        };
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitIdentifier_Diagnostic()
        {
            var testCode = @"class C
{
    void M(int x)
    {
        var result = new
        {
            {|GM0131:x|}
        };
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleImplicitFields_MultipleDiagnostics()
        {
            var testCode = @"class W { public string key; public string name; }
class C
{
    void M(W w)
    {
        var result = new
        {
            {|GM0131:w.key|},
            {|GM0131:w.name|}
        };
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
