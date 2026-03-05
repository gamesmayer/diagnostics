namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0005Analyzer>;

    public class GM0005AnalyzerTests
    {
        [Fact]
        public async Task SingleAttribute_NoDiagnostic()
        {
            var testCode = @"
[System.Serializable]
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleAttributesInSeparateBrackets_NoDiagnostic()
        {
            var testCode = @"
[System.Serializable]
[System.Obsolete]
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TwoAttributesSeparatedByComma_Diagnostic()
        {
            var testCode = @"
{|GM0005:[System.Serializable, System.Obsolete]|}
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeAttributesSeparatedByComma_Diagnostic()
        {
            var testCode = @"
{|GM0005:[System.Serializable, System.Obsolete, System.CLSCompliant(true)]|}
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AttributesOnMethod_Diagnostic()
        {
            var testCode = @"
class Foo
{
    {|GM0005:[System.Obsolete, System.CLSCompliant(true)]|}
    public void Bar() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AttributesOnProperty_Diagnostic()
        {
            var testCode = @"
class Foo
{
    {|GM0005:[System.Obsolete, System.CLSCompliant(true)]|}
    public int Bar { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AttributesOnField_Diagnostic()
        {
            var testCode = @"
class Foo
{
    {|GM0005:[System.Obsolete, System.CLSCompliant(true)]|}
    private int _bar;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
