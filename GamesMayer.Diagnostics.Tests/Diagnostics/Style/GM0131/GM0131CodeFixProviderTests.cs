namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0131CodeFixProviderTests
    {
        [Fact]
        public async Task ImplicitMemberAccess_FixToExplicit()
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

            var fixedCode = @"class W { public string key; public string mapPrefab; }
class C
{
    void M(W w)
    {
        var result = new
        {
            prefab = w.mapPrefab,
            key = w.key
        };
    }
}";

            var test = new CSharpCodeFixTest<GM0131Analyzer, GM0131CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ImplicitIdentifier_FixToExplicit()
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

            var fixedCode = @"class C
{
    void M(int x)
    {
        var result = new
        {
            x = x
        };
    }
}";

            var test = new CSharpCodeFixTest<GM0131Analyzer, GM0131CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
