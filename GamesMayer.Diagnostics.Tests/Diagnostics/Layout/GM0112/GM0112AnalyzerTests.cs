namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0112Analyzer>;

    public class GM0112AnalyzerTests
    {
        [Fact]
        public async Task SingleDeclaration_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int a = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConsecutiveDeclarationsWithoutBlankLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int a = 1;
        int b = 2;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBetweenConsecutiveDeclarations_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int a = 1;
{|GM0112:
|}        int b = 2;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenConsecutiveDeclarations_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    void M()
    {
        int a = 1;
{|GM0112:
|}{|GM0112:
|}        int b = 2;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonConsecutiveDeclarations_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int a = 1;

        DoWork();

        int b = 2;
    }

    void DoWork()
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CommentLineBetweenConsecutiveDeclarations_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int a = 1;
        // keep grouped values clear
        int b = 2;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}