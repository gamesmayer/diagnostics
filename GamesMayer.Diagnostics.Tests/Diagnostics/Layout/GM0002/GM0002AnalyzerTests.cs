namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0002Analyzer>;

    public class GM0002AnalyzerTests
    {
        [Fact]
        public async Task SingleLineAutoProperty_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public string Name { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyWithGetterBody_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public int Count
    {
        get { return 0; }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineAutoProperty_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0002:public string Name
    {
        get;
        set;
    }|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineAutoPropertyWithAttribute_DiagnosticExcludesAttribute()
        {
            var testCode = @"using System;
class Foo
{
    [Obsolete]
    {|GM0002:public string Name
    {
        get;
        set;
    }|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
