namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM1508CodeFixProviderTests
    {
        [Fact]
        public async Task ClassDeclaration_BlankLineBeforeCloseBrace_Fix()
        {
            var testCode = @"class Foo
{
    void Method() { }
{|GM1508:|}
}";
            var fixedCode = @"class Foo
{
    void Method() { }
}";
            var test = new CSharpCodeFixTest<GM1508Analyzer, GM1508CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodBody_BlankLineBeforeCloseBrace_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
{|GM1508:|}
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM1508Analyzer, GM1508CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLinesBeforeCloseBrace_Fix()
        {
            var testCode = "class Foo\n{\n    void Method() { }\n{|GM1508:|}\n{|GM1508:|}\n}";
            var fixedCode = "class Foo\n{\n    void Method() { }\n}";
            var test = new CSharpCodeFixTest<GM1508Analyzer, GM1508CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfIncrementalIterations = 2,
            };
            await test.RunAsync();
        }
    }
}
