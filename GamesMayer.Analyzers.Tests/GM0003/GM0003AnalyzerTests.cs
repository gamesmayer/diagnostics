namespace GamesMayer.Analyzers.Tests.GM0003
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM0003Analyzer>;

    public class GM0003AnalyzerTests
    {
        [Fact]
        public async Task AttributeDirectlyBeforeMember_NoDiagnostic()
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
        public async Task BlankLineAfterAttributeOnProperty_Diagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
{|GM0003:
|}    public string Name { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineAfterAttributeOnMethod_Diagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
{|GM0003:
|}    public void Bar() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineAfterAttributeOnField_Diagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
{|GM0003:
|}    public string Name;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineAfterAttributeOnClass_Diagnostic()
        {
            var testCode = @"[System.Obsolete]
{|GM0003:
|}public class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineAfterAttributeOnConstructor_Diagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
{|GM0003:
|}    public Foo() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
