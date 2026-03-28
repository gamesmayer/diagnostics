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

        [Fact]
        public async Task InterfaceMembersSeparatedByBlankLine_NoDiagnostic()
        {
            var testCode = @"interface IWallet
{
    int Amount { get; }

    void Earn(int amount);
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InterfaceMembersWithoutBlankLine_Diagnostic()
        {
            var testCode = @"interface IWallet
{
    int Amount { get; }
    {|GM0007:void|} Earn(int amount);
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
