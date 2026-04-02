namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0092Analyzer>;

    public class GM0092AnalyzerTests
    {
        [Fact]
        public async Task NoSpace_InvocationExpression_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M() { }
    void N() { M(); }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task HasSpace_InvocationExpression_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M() { }
    void N() { M{|GM0092:(|} ); }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpace_InvocationExpression_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M() { }
    void N() { M{|GM0092:(|}); }
}";

            var test = new CSharpAnalyzerTest<GM0092Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0092.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task HasSpace_InvocationExpression_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M() { }
    void N() { M( ); }
}";

            var test = new CSharpAnalyzerTest<GM0092Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0092.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_ObjectCreation_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M() { var f = new Foo(); }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task HasSpace_ObjectCreation_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M() { var f = new Foo{|GM0092:(|} ); }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonEmptyArgumentList_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(int value) { }
    void N() { M(1); }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
