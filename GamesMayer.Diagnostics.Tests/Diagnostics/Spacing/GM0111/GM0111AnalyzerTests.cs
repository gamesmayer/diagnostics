namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0111Analyzer>;

    public class GM0111AnalyzerTests
    {
        [Fact]
        public async Task StatementSemicolonAdjacent_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        return;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeStatementSemicolon_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0 {|GM0111:;|}
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NewLineBeforeStatementSemicolon_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0
{|GM0111:;|}
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SemicolonsInForHeader_AreIgnored()
        {
            var testCode = @"class Foo
{
    void M()
    {
        for (int i = 0 ; i < 10 ; i++) { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldSemicolonAdjacent_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int _value;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeFieldSemicolon_Diagnostic()
        {
            var testCode = @"class Foo
{
    private int _value {|GM0111:;|}
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
