namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0047Analyzer>;

    public class GM0047AnalyzerTests
    {
        [Fact]
        public async Task SingleLine_Assignment_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        string x = null;
        x = ""value"";
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleLine_Declaration_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        string x = ""value"";
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLine_ValueStartsOnSameLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(string a, string b)
    {
        string x = a +
            b;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLine_Assignment_ValueStartsOnSameLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(string a, string b)
    {
        string x = null;
        x = a +
            b;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DefaultParameter_MultiLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(int x = 5) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Assignment_ValueOnNextLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        string defaultCharacter = null;
        defaultCharacter =
            {|GM0047:""value""|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Declaration_ValueOnNextLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        string x =
            {|GM0047:""value""|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Assignment_MultiLineValue_ValueStartsOnNextLine_Diagnostic()
        {
            var testCode = @"class C
{
    string[] characters = null;
    void M()
    {
        string defaultCharacter = null;
        defaultCharacter =
            {|GM0047:characters[0]|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldDeclaration_ValueOnNextLine_Diagnostic()
        {
            var testCode = @"class C
{
    private string x =
        {|GM0047:""value""|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldDeclaration_ArrayInitializerOnNextLine_NoDiagnostic()
        {
            var testCode = @"class I18nLocaleSettings { public I18nLocaleSettings(string a, string b) { } }
class C
{
    public I18nLocaleSettings[] locales =
    { new I18nLocaleSettings(""en"", ""EN_NAME"") };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

    }
}
