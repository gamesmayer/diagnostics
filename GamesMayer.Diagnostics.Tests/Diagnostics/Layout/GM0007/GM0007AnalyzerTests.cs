namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0007Analyzer>;

    public class GM0007AnalyzerTests
    {
        [Fact]
        public async Task MethodsSeparatedByBlankLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void A() { }

    void B() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodsWithoutBlankLine_Diagnostic()
        {
            var testCode = @"class C
{
    void A() { }
    {|GM0007:void|} B() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConsecutiveFieldsWithoutBlankLine_Diagnostic()
        {
            var testCode = @"class C
{
    private int first;
    {|GM0007:private|} int second;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConsecutiveFieldsWithBlankLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    private int first;

    private int second;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
