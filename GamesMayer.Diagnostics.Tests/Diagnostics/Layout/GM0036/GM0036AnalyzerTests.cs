namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0036Analyzer>;

    public class GM0036AnalyzerTests
    {
        [Fact]
        public async Task SingleLineDeclarationWithSingleLineValue_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int value = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleLineDeclarationWithMultiLineValue_NoDiagnostic()
        {
            var testCode = @"using System.Linq;

class C
{
    void M(int[] numbers)
    {
        var value = numbers
            .Where(x => x > 0)
            .Select(x => x * 2)
            .ToArray();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TypeAndIdentifierOnDifferentLines_DiagnosticOnEntireStatement()
        {
            var testCode = @"using System.Linq;

class C
{
    void M(int[] numbers)
    {
        {|GM0036:var
        value = numbers
            .Where(x => x > 0)
            .Select(x => x * 2)
            .ToArray();|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IdentifierAndEqualsOnDifferentLines_DiagnosticOnEntireStatement()
        {
            var testCode = @"class C
{
    void M()
    {
        {|GM0036:int value
            = 1;|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DeclarationWithoutInitializerOnDifferentLines_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        {|GM0036:int
        value;|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
