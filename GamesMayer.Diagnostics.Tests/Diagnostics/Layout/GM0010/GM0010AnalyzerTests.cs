namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0010Analyzer>;

    public class GM0010AnalyzerTests
    {
        [Fact]
        public async Task NoBlankLines_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleBlankLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{

    void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TwoConsecutiveBlankLines_Diagnostic()
        {
            var testCode = @"class Foo
{

{|GM0010:|}
    void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeConsecutiveBlankLines_TwoDiagnostics()
        {
            var testCode = @"class Foo
{

{|GM0010:|}
{|GM0010:|}
    void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TwoConsecutiveBlankLinesBetweenMembers_Diagnostic()
        {
            var testCode = @"class Foo
{
    void A() { }

{|GM0010:|}
    void B() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TwoConsecutiveBlankLinesAtTopLevel_Diagnostic()
        {
            var testCode = @"using System;

{|GM0010:|}
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
