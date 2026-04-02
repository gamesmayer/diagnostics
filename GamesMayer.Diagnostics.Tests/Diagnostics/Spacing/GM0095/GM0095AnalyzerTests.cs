namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0095Analyzer>;

    public class GM0095AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceBeforeComma_ArgumentList_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a, int b) { }
    void N() { M(1, 2); }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeComma_ArgumentList_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a, int b) { }
    void N() { M(1 {|GM0095:,|} 2); }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBeforeComma_ParameterList_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a, int b) { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeComma_ParameterList_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a {|GM0095:,|} int b) { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBeforeComma_TypeArgumentList_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    Dictionary<int, string> d = new();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeComma_TypeArgumentList_DefaultSetting_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    Dictionary<int {|GM0095:,|} string> d = new();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBeforeComma_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a{|GM0095:,|} int b) { }
    void N() { M(1{|GM0095:,|} 2); }
}";

            var test = new CSharpAnalyzerTest<GM0095Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0095.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBeforeComma_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(int a , int b) { }
    void N() { M(1 , 2); }
}";

            var test = new CSharpAnalyzerTest<GM0095Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0095.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task CommaFollowedByPrecedingNewline_DefaultSetting_NoDiagnostic()
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
