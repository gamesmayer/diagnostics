namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0103Analyzer>;

    public class GM0103AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceBeforeLessThan_TypeParameter_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo<T> { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeLessThan_TypeParameter_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo {|GM0103:<|}T> { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBeforeLessThan_MethodTypeParameter_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M<T>() { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeLessThan_MethodTypeParameter_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M {|GM0103:<|}T>() { }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBeforeLessThan_TypeArgument_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    System.Collections.Generic.List<int> Values = new System.Collections.Generic.List<int>();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeLessThan_TypeArgument_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    System.Collections.Generic.List {|GM0103:<|}int> Values = new System.Collections.Generic.List<int>();
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeLessThan_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M <T>() { }
}";

            var test = new CSharpAnalyzerTest<GM0103Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0103.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBeforeLessThan_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M{|GM0103:<|}T>() { }
}";

            var test = new CSharpAnalyzerTest<GM0103Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0103.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBeforeLessThan_PropertyReturnType_DefaultSetting_Diagnostic()
        {
            var testCode = @"class TaskProgressEntity { }

class Foo
{
    private System.Collections.Generic.List<TaskProgressEntity> revealingTasks = new();
    public System.Collections.Generic.IReadOnlyList {|GM0103:<|}TaskProgressEntity> RevealingTasks => revealingTasks;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NewLineBeforeLessThan_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
<T> { }";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
