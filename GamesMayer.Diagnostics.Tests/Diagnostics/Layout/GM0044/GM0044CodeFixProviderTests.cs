namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0044CodeFixProviderTests
    {
        [Fact]
        public async Task ExpressionBody_WithBlankLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
{|GM0044:|}
            x + 1;
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
            x + 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0044Analyzer, GM0044CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task BlockBody_WithBlankLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
{|GM0044:|}
        {
            return x + 1;
        };
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
        {
            return x + 1;
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0044Analyzer, GM0044CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLines_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
{|GM0044:|}

            x + 1;
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
            x + 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0044Analyzer, GM0044CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
