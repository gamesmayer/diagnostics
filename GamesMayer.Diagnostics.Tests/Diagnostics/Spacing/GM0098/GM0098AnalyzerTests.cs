namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0098Analyzer>;

    public class GM0098AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceBeforeOpenBracket_ElementAccess_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int Bar(int[] arr) => arr[0];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeOpenBracket_ElementAccess_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int Bar(int[] arr) => arr {|GM0098:[|}0];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBeforeOpenBracket_ArrayType_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int[] arr = new int[3];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeOpenBracket_ArrayType_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int {|GM0098:[|}] arr = new int[3];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeOpenBracket_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s [0];
}";

            var test = new CSharpAnalyzerTest<GM0098Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0098.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBeforeOpenBracket_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s{|GM0098:[|}0];
}";

            var test = new CSharpAnalyzerTest<GM0098Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0098.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task Attribute_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"using System;
[Obsolete]
class Foo { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
