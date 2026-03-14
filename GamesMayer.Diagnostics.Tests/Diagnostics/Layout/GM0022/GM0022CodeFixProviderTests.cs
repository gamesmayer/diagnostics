namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0022CodeFixProviderTests
    {
        [Fact]
        public async Task EmptyMethodBraceOnSameLine_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        void Local() {|GM0022:{|} }
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        void Local()
        { }
    }
}";
            var test = new CSharpCodeFixTest<GM0022Analyzer, GM0022CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyTypeBraceOnSameLine_Fix()
        {
            var testCode = @"class Foo {|GM0022:{|} }
";
            var fixedCode = @"class Foo
{ }
";
            var test = new CSharpCodeFixTest<GM0022Analyzer, GM0022CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyControlBlockBraceOnSameLine_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true) {|GM0022:{|} }
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        if (true)
        { }
    }
}";
            var test = new CSharpCodeFixTest<GM0022Analyzer, GM0022CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
