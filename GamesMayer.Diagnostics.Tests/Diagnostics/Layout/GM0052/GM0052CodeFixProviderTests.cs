namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0052CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineBetweenReturnAndExpression_Fix()
        {
            var testCode = @"class C
{
    int M()
    {
        return
{|GM0052:
|}            42;
    }
}";
            var fixedCode = @"class C
{
    int M()
    {
        return
            42;
    }
}";

            var test = new CSharpCodeFixTest<GM0052Analyzer, GM0052CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenReturnAndExpression_Fix()
        {
            var testCode = @"class C
{
    int[] M(int[] items)
    {
        return
{|GM0052:
|}{|GM0052:
|}            items;
    }
}";
            var fixedCode = @"class C
{
    int[] M(int[] items)
    {
        return
            items;
    }
}";

            var test = new CSharpCodeFixTest<GM0052Analyzer, GM0052CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfIncrementalIterations = 2,
            };

            await test.RunAsync();
        }
    }
}
