namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0029CodeFixProviderTests
    {
        [Fact]
        public async Task MultiLineArgumentList_OpenParenOnDeclarationLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo{|GM0029:(|}
            1,
            2,
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            var fixedCode = @"class C
{
    void M()
    {
        Foo
        (
            1,
            2,
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            var test = new CSharpCodeFixTest<GM0029Analyzer, GM0029CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultiLineParameterList_OpenParenOnDeclarationLine_Fix()
        {
            var testCode = @"class C
{
    void M{|GM0029:(|}
        int a,
        int b,
        int c
    )
    {
    }
}";
            var fixedCode = @"class C
{
    void M
    (
        int a,
        int b,
        int c
    )
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0029Analyzer, GM0029CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
