namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0004Analyzer>;

    public class GM0004AnalyzerTests
    {
        [Fact]
        public async Task AttributeOnSeparateLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
    public string Name { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MemberWithoutAttribute_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public string Name { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AttributeAndPropertyOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete] {|GM0004:public|} string Name { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AttributeAndMethodOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete] {|GM0004:public|} void Bar() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AttributeAndFieldOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete] {|GM0004:public|} string Name;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AttributeAndClassOnSameLine_Diagnostic()
        {
            var testCode = @"[System.Obsolete] {|GM0004:public|} class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
