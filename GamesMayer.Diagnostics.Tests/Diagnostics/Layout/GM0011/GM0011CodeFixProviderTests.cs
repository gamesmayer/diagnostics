namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0011CodeFixProviderTests
    {
        [Fact]
        public async Task OneBlankLineAtStart_Fix()
        {
            var testCode = "{|GM0011:|}\nusing System;\nclass Foo { }";
            var fixedCode = "using System;\nclass Foo { }";
            var test = new CSharpCodeFixTest<GM0011Analyzer, GM0011CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task TwoBlankLinesAtStart_Fix()
        {
            var testCode = "{|GM0011:|}\n{|GM0011:|}\nusing System;\nclass Foo { }";
            var fixedCode = "using System;\nclass Foo { }";
            var test = new CSharpCodeFixTest<GM0011Analyzer, GM0011CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
