namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0012CodeFixProviderTests
    {
        [Fact]
        public async Task ClassDeclaration_BlankLineBeforeBrace_Fix()
        {
            var testCode = @"class Foo
{|GM0012:|}
{
}";
            var fixedCode = @"class Foo
{
}";
            var test = new CSharpCodeFixTest<GM0012Analyzer, GM0012CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task IfStatement_BlankLineBeforeBrace_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
{|GM0012:|}
        {
        }
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0012Analyzer, GM0012CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLinesBeforeBrace_Fix()
        {
            var testCode = @"class Foo

{|GM0012:|}
{
}";
            var fixedCode = @"class Foo

{
}";
            var test = new CSharpCodeFixTest<GM0012Analyzer, GM0012CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
