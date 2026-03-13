namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0016CodeFixProviderTests
    {
        [Fact]
        public async Task ClassAttribute_MultiLine_Fix()
        {
            var testCode = @"{|GM0016:[
    System.Obsolete
]|}
class Foo { }";
            var fixedCode = @"[System.Obsolete]
class Foo { }";
            var test = new CSharpCodeFixTest<GM0016Analyzer, GM0016CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodAttribute_MultiLine_Fix()
        {
            var testCode = @"class Foo
{
    {|GM0016:[
        System.Obsolete
    ]|}
    void Method() { }
}";
            var fixedCode = @"class Foo
{
    [System.Obsolete]
    void Method() { }
}";
            var test = new CSharpCodeFixTest<GM0016Analyzer, GM0016CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
