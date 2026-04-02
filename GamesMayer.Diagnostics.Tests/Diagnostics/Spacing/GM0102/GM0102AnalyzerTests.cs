namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0102Analyzer>;

    public class GM0102AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceBetweenBrackets_ElementAccess_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s[0];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBetweenBrackets_ElementAccess_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s{|GM0102:[|} 0 ];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceOnlyAfterOpenBracket_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s{|GM0102:[|} 0];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceOnlyBeforeCloseBracket_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s{|GM0102:[|}0 ];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBetweenBrackets_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s[ 0 ];
}";

            var test = new CSharpAnalyzerTest<GM0102Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0102.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBetweenBrackets_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s{|GM0102:[|}0];
}";

            var test = new CSharpAnalyzerTest<GM0102Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0102.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyBrackets_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int[] arr = new int[3];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultilineContent_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s[
        0];
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBetweenBrackets_Attribute_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"using System;
[Obsolete]
class Foo { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBetweenBrackets_Attribute_DefaultSetting_Diagnostic()
        {
            var testCode = @"using System;
{|GM0102:[|} Obsolete ]
class Foo { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBetweenBrackets_Attribute_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"using System;
[ Obsolete ]
class Foo { }";

            var test = new CSharpAnalyzerTest<GM0102Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0102.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBetweenBrackets_Attribute_EnabledTrue_Diagnostic()
        {
            var testCode = @"using System;
{|GM0102:[|}Obsolete]
class Foo { }";

            var test = new CSharpAnalyzerTest<GM0102Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0102.enabled = true"));

            await test.RunAsync();
        }
    }
}
