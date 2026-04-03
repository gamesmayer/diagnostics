namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0124CodeFixProviderTests
    {
        [Fact]
        public async Task EnumTrailingComma_Fix()
        {
            var testCode = @"enum Value
{
    A,
    B{|GM0124:,|}
}";

            var fixedCode = @"enum Value
{
    A,
    B
}";

            var test = new CSharpCodeFixTest<GM0124Analyzer, GM0124CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ArrayInitializerTrailingComma_Fix()
        {
            var testCode = @"class C
{
    int[] values = new[]
    {
        1,
        2{|GM0124:,|}
    };
}";

            var fixedCode = @"class C
{
    int[] values = new[]
    {
        1,
        2
    };
}";

            var test = new CSharpCodeFixTest<GM0124Analyzer, GM0124CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}