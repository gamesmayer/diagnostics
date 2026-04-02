namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0033CodeFixProviderTests
    {
        [Fact]
        public async Task MultiLineArgumentList_AllmanOpenParenOverIndented_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
          {|GM0033:(|}
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
            var test = new CSharpCodeFixTest<GM0033Analyzer, GM0033CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultiLineParameterList_AllmanOpenParenUnderIndented_Fix()
        {
            var testCode = @"class C
{
    void M
{|GM0033:(|}
        int a,
        int b
    )
    {
    }
}";
            var fixedCode = @"class C
{
    void M
    (
        int a,
        int b
    )
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0033Analyzer, GM0033CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}