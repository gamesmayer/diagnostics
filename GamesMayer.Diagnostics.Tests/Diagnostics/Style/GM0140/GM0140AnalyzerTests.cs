namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0140Analyzer>;

    public class GM0140AnalyzerTests
    {
        [Fact]
        public async Task UnusedUsing_Diagnostic()
        {
            var testCode = @"{|GM0140:using System.Text;|}
class C { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UsedUsing_NoDiagnostic()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        Console.WriteLine();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleUnusedUsings_MultipleDiagnostics()
        {
            var testCode = @"{|GM0140:using System.Text;|}
{|GM0140:using System.Collections.Generic;|}
class C { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MixedUsedAndUnused_OnlyUnusedReported()
        {
            var testCode = @"using System;
{|GM0140:using System.Text;|}
class C
{
    void M()
    {
        Console.WriteLine();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
