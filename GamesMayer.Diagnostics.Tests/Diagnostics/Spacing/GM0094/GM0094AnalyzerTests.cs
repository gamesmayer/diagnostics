namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0094Analyzer>;

    public class GM0094AnalyzerTests
    {
        [Fact]
        public async Task SpaceAfterComma_ArgumentList_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a, int b) { }
    void N() { M(1, 2); }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceAfterComma_ArgumentList_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a, int b) { }
    void N() { M(1{|GM0094:,|}2); }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterComma_ParameterList_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a, int b) { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceAfterComma_ParameterList_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a{|GM0094:,|}int b) { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterComma_TypeArgumentList_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    Dictionary<int, string> d = new();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceAfterComma_TypeArgumentList_DefaultSetting_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    Dictionary<int{|GM0094:,|}string> d = new();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterComma_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a,int b) { }
    void N() { M(1{|GM0094:,|} 2); }
}";

            var test = new CSharpAnalyzerTest<GM0094Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0094.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceAfterComma_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a,int b) { }
    void N() { M(1,2); }
}";

            var test = new CSharpAnalyzerTest<GM0094Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0094.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task CommaFollowedByNewline_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(
        int a,
        int b) { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
