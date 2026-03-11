namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0011Analyzer>;

    public class GM0011AnalyzerTests
    {
        [Fact]
        public async Task NoBlankLineAtStart_NoDiagnostic()
        {
            var testCode = @"using System;
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineAtStart_Diagnostic()
        {
            var testCode = "{|GM0011:|}\nusing System;\nclass Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TwoBlankLinesAtStart_TwoDiagnostics()
        {
            var testCode = "{|GM0011:|}\n{|GM0011:|}\nusing System;\nclass Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineNotAtStart_NoDiagnostic()
        {
            var testCode = @"using System;

class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
