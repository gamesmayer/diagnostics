namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0030CodeFixProviderTests
    {
        [Fact]
        public async Task MultiLineArgumentList_BlankLineBetweenDeclarationAndOpenParen_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
{|GM0030:|}
        (
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
            var test = new CSharpCodeFixTest<GM0030Analyzer, GM0030CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultiLineParameterList_BlankLineBetweenDeclarationAndOpenParen_Fix()
        {
            var testCode = @"class C
{
    void M
{|GM0030:|}
    (
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
            var test = new CSharpCodeFixTest<GM0030Analyzer, GM0030CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyArgumentList_BlankLineBetweenDeclarationAndOpenParen_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Unregister
{|GM0030:|}
        ();
    }

    void Unregister() { }
}";
            var fixedCode = @"class C
{
    void M()
    {
        Unregister
        ();
    }

    void Unregister() { }
}";
            var test = new CSharpCodeFixTest<GM0030Analyzer, GM0030CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyParameterList_BlankLineBetweenDeclarationAndOpenParen_Fix()
        {
            var testCode = @"class C
{
    void M
{|GM0030:|}
    ()
    {
    }
}";
            var fixedCode = @"class C
{
    void M
    ()
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0030Analyzer, GM0030CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
