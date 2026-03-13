namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0016Analyzer>;

    public class GM0016AnalyzerTests
    {
        [Fact]
        public async Task ClassAttribute_SingleLine_NoDiagnostic()
        {
            var testCode = @"[System.Obsolete]
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodAttribute_SingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    [System.Obsolete]
    void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassAttribute_MultiLine_Diagnostic()
        {
            var testCode = @"{|GM0016:[
    System.Obsolete
]|}
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodAttribute_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0016:[
        System.Obsolete
    ]|}
    void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyAttribute_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0016:[
        System.Obsolete
    ]|}
    int Value { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleAttributes_OneSingleLineOneMultiLine_OnlyMultiLineFlagged()
        {
            var testCode = @"[System.Serializable]
{|GM0016:[
    System.Obsolete
]|}
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
