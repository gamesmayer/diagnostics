namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0124Analyzer>;

    public class GM0124AnalyzerTests
    {
        [Fact]
        public async Task EnumTrailingComma_Diagnostic()
        {
            var testCode = @"enum Value
{
    A,
    B{|GM0124:,|}
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializerTrailingComma_Diagnostic()
        {
            var testCode = @"class C
{
    int[] values = new[]
    {
        1,
        2{|GM0124:,|}
    };
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializerTrailingComma_Diagnostic()
        {
            var testCode = @"class C
{
    class Item { public int X { get; set; } }

    Item value = new Item
    {
        X = 1{|GM0124:,|}
    };
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonTrailingCommas_NoDiagnostic()
        {
            var testCode = @"class C
{
    enum Value
    {
        A,
        B
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}