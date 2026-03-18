namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM1505CodeFixProviderTests
    {
        [Fact]
        public async Task ClassDeclaration_BlankLineAfterOpenBrace_Fix()
        {
            var testCode = @"class Foo
{
{|GM1505:|}
    void Method() { }
}";
            var fixedCode = @"class Foo
{
    void Method() { }
}";
            var test = new CSharpCodeFixTest<GM1505Analyzer, GM1505CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodBody_BlankLineAfterOpenBrace_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
{|GM1505:|}
        int x = 1;
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM1505Analyzer, GM1505CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLinesAfterOpenBrace_Fix()
        {
            var testCode = "class Foo\n{\n{|GM1505:|}\n{|GM1505:|}\n    void Method() { }\n}";
            var fixedCode = "class Foo\n{\n    void Method() { }\n}";
            var test = new CSharpCodeFixTest<GM1505Analyzer, GM1505CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfIncrementalIterations = 2,
            };
            await test.RunAsync();
        }
    }
}
