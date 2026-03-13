namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0021CodeFixProviderTests
    {
        [Fact]
        public async Task EmptyMethodBraceOnNewLine_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {|GM0021:{|} }
}";
            var fixedCode = @"class Foo
{
    void Method() { }
}";
            var test = new CSharpCodeFixTest<GM0021Analyzer, GM0021CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyTypeBraceOnNewLine_Fix()
        {
            var testCode = @"class Foo
{|GM0021:{|} }
";
            var fixedCode = @"class Foo { }
";
            var test = new CSharpCodeFixTest<GM0021Analyzer, GM0021CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyControlBlockBraceOnNewLine_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {|GM0021:{|} }
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        if (true) { }
    }
}";
            var test = new CSharpCodeFixTest<GM0021Analyzer, GM0021CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
