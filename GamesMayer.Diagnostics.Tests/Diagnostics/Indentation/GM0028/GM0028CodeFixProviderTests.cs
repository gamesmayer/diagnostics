namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0028CodeFixProviderTests
    {
        [Fact]
        public async Task MultiLineArgumentList_CloseParenOverIndented_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3
            {|GM0028:)|};
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
            var test = new CSharpCodeFixTest<GM0028Analyzer, GM0028CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultiLineParameterList_CloseParenUnderIndented_Fix()
        {
            var testCode = @"class C
{
    void M(
        int a,
        int b,
        int c
{|GM0028:)|}
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
            var test = new CSharpCodeFixTest<GM0028Analyzer, GM0028CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
