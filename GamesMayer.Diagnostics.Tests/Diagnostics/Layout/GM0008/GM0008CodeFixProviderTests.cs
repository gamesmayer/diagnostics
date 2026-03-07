namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0008CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineBetweenArguments_Fix()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        Baz(
            1,
{|GM0008:
|}            2);
    }

    void Baz(int a, int b) { }
}";
            var fixedCode = @"class Foo
{
    void Bar()
    {
        Baz(
            1,
            2);
    }

    void Baz(int a, int b) { }
}";
            var test = new CSharpCodeFixTest<GM0008Analyzer, GM0008CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task BlankLineAfterOpenParenAndBeforeCloseParen_Fix()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        Baz(
{|GM0008:
|}            1,
            2
{|GM0008:
|}        );
    }

    void Baz(int a, int b) { }
}";
            var fixedCode = @"class Foo
{
    void Bar()
    {
        Baz(
            1,
            2
        );
    }

    void Baz(int a, int b) { }
}";
            var test = new CSharpCodeFixTest<GM0008Analyzer, GM0008CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodDeclarationBlankLineBetweenParameters_Fix()
        {
            var testCode = @"class Foo
{
    public void Bar(
        int a,
{|GM0008:
|}        int b,
        int c)
    {
    }
}";
            var fixedCode = @"class Foo
{
    public void Bar(
        int a,
        int b,
        int c)
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0008Analyzer, GM0008CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
