namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0026CodeFixProviderTests
    {
        [Fact]
        public async Task MultiLineArgumentList_CloseParenOnLastItemLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3{|GM0026:)|};
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
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            var test = new CSharpCodeFixTest<GM0026Analyzer, GM0026CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultiLineParameterList_CloseParenOnLastItemLine_Fix()
        {
            var testCode = @"class C
{
    void M(
        int a,
        int b,
        int c{|GM0026:)|}
    {
    }
}";
            var fixedCode = @"class C
{
    void M(
        int a,
        int b,
        int c
    )
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0026Analyzer, GM0026CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultiLineArgumentList_BlankLineBeforeCloseParen_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3

        {|GM0026:)|};
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
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            var test = new CSharpCodeFixTest<GM0026Analyzer, GM0026CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}