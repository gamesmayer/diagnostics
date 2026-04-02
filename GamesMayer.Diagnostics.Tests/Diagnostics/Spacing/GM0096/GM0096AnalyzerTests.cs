namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0096Analyzer>;

    public class GM0096AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceAfterDot_MemberAccess_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int Bar => ""hello"".Length;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterDot_MemberAccess_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int Bar => ""hello""{|GM0096:.|} Length;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceAfterDot_QualifiedName_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"using System.Collections.Generic;
class Foo { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterDot_QualifiedName_DefaultSetting_Diagnostic()
        {
            var testCode = @"using System{|GM0096:.|} Collections;
class Foo { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterDot_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int Bar => ""hello"". Length;
}";

            var test = new CSharpAnalyzerTest<GM0096Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0096.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceAfterDot_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    int Bar => ""hello""{|GM0096:.|}Length;
}";

            var test = new CSharpAnalyzerTest<GM0096Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0096.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task DotFollowedByNewline_DefaultSetting_NoDiagnostic()
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
