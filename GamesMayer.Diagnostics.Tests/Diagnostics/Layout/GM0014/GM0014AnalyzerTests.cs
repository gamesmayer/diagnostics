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
        public async Task Namespace_KeywordOnDifferentLineThanIdentifier_Diagnostic()
        {
            var testCode = @"{|GM0014:namespace
    Foo.Bar|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Namespace_KeywordOnDifferentLineThanThreeSegmentIdentifier_Diagnostic()
        {
            var testCode = @"{|GM0014:namespace
    Foo.Bar.Baz|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FileScopedNamespace_KeywordOnDifferentLineThanIdentifier_Diagnostic()
        {
            var testCode = @"{|GM0014:namespace
    Foo.Bar|};";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Namespace_IdentifierSpansMultipleLines_Diagnostic()
        {
            var testCode = @"{|GM0014:namespace Foo
    .Bar|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FileScopedNamespace_IdentifierSpansMultipleLines_Diagnostic()
        {
            var testCode = @"{|GM0014:namespace Foo
    .Bar|};";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
