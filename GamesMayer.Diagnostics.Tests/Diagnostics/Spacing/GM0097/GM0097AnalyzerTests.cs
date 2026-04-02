namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0097Analyzer>;

    public class GM0097AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceBeforeDot_MemberAccess_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int Bar => ""hello"".Length;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeDot_MemberAccess_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int Bar => ""hello"" {|GM0097:.|} Length;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBeforeDot_QualifiedName_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeDot_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int Bar => ""hello"" .Length;
}";

            var test = new CSharpAnalyzerTest<GM0097Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0097.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBeforeDot_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    int Bar => ""hello""{|GM0097:.|} Length;
}";

            var test = new CSharpAnalyzerTest<GM0097Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0097.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task DotPrecededByNewline_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    string Bar => ""hello""
        .ToString();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
