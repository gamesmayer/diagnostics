namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0141Analyzer>;

    public class GM0141AnalyzerTests
    {
        [Fact]
        public async Task NoContinue_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        for (int i = 0; i < 10; i++)
        {
            if (i % 2 == 0)
            {
                // do nothing
            }
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ContinueInForLoop_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        for (int i = 0; i < 10; i++)
        {
            if (i % 2 == 0)
                {|GM0141:continue;|}
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ContinueInWhileLoop_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int i = 0;
        while (i < 10)
        {
            i++;
            if (i % 2 == 0)
                {|GM0141:continue;|}
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ContinueInForeachLoop_Diagnostic()
        {
            var testCode = @"class C
{
    void M(int[] items)
    {
        foreach (var item in items)
        {
            if (item == 0)
                {|GM0141:continue;|}
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleContinues_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    void M(int[] items)
    {
        foreach (var item in items)
        {
            if (item == 0)
                {|GM0141:continue;|}
            if (item < 0)
                {|GM0141:continue;|}
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
