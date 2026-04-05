namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0137Analyzer>;

    public class GM0137AnalyzerTests
    {
        [Fact]
        public async Task CastAndOperandOnSameLine_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    enum PlayerType { None }

    void Test()
    {
        int index = 0;
        var result = (PlayerType)index;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CastAndOperandOnSameLine_WithParenthesizedExpr_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    void Test()
    {
        int index = 0;
        var result = (float)(index + 1);
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CastAndOperandOnNextLine_Diagnostic()
        {
            var testCode = @"
class Foo
{
    enum PlayerType { None }

    void Test()
    {
        int index = 0;
        var result = {|GM0137:(PlayerType)
index|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CastAndOperandSeparatedByBlankLine_Diagnostic()
        {
            var testCode = @"
class Foo
{
    enum PlayerType { None }

    void Test()
    {
        int index = 0;
        var result = {|GM0137:(PlayerType)

index|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CastWithIndentedOperandOnNextLine_Diagnostic()
        {
            var testCode = @"
class Foo
{
    void Test()
    {
        int index = 0;
        var result = {|GM0137:(float)
            index|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
