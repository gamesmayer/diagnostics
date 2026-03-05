namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0006Analyzer>;

    public class GM0006AnalyzerTests
    {
        [Fact]
        public async Task TwoAttributesConsecutive_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
    [System.CLSCompliant(true)]
    public string Name { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleAttribute_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
    public string Name { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBetweenAttributesOnProperty_Diagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
{|GM0006:
|}    [System.CLSCompliant(true)]
    public string Name { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBetweenAttributesOnMethod_Diagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
{|GM0006:
|}    [System.CLSCompliant(true)]
    public void Bar() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBetweenAttributesOnClass_Diagnostic()
        {
            var testCode = @"[System.Obsolete]
{|GM0006:
|}[System.Serializable]
public class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
