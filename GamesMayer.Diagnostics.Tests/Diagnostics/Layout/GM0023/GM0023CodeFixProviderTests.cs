namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0023CodeFixProviderTests
    {
        [Fact]
        public async Task WronglyIndentedArgument_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
        {|GM0023:2|},
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            var fixedCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            var test = new CSharpCodeFixTest<GM0023Analyzer, GM0023CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task WronglyIndentedParameter_Fix()
        {
            var testCode = @"class C
{
    void M(
        int a,
    {|GM0023:int b|},
        int c)
    {
    }
}";
            var fixedCode = @"class C
{
    void M(
        int a,
        int b,
        int c)
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0023Analyzer, GM0023CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task WronglyIndentedNamedArgument_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            a: 1,
        {|GM0023:b: 2|},
            c: 3);
    }

    void Foo(int a, int b, int c) { }
}";
            var fixedCode = @"class C
{
    void M()
    {
        Foo(
            a: 1,
            b: 2,
            c: 3);
    }

    void Foo(int a, int b, int c) { }
}";
            var test = new CSharpCodeFixTest<GM0023Analyzer, GM0023CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
