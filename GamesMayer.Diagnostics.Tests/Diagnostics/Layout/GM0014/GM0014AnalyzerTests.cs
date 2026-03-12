namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0014Analyzer>;

    public class GM0014AnalyzerTests
    {
        [Fact]
        public async Task Namespace_SingleLine_NoDiagnostic()
        {
            var testCode = @"namespace Foo.Bar.Baz
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Namespace_SimpleIdentifier_NoDiagnostic()
        {
            var testCode = @"namespace Foo
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FileScopedNamespace_SingleLine_NoDiagnostic()
        {
            var testCode = @"namespace Foo.Bar.Baz;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Namespace_NameOnMultipleLines_Diagnostic()
        {
            var testCode = @"namespace {|GM0014:Foo
    .Bar|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Namespace_ThreeSegmentsOnMultipleLines_Diagnostic()
        {
            var testCode = @"namespace {|GM0014:Foo
    .Bar
    .Baz|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FileScopedNamespace_NameOnMultipleLines_Diagnostic()
        {
            var testCode = @"namespace {|GM0014:Foo
    .Bar|};";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
