namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0092CodeFixProviderTests
    {
        [Fact]
        public async Task HasSpace_InvocationExpression_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    void M() { }
    void N() { M{|GM0092:(|} ); }
}";
            var fixedCode = @"class Foo
{
    void M() { }
    void N() { M(); }
}";

            var test = new CSharpCodeFixTest<GM0092Analyzer, GM0092CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_InvocationExpression_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    void M() { }
    void N() { M{|GM0092:(|}); }
}";
            var fixedCode = @"class Foo
{
    void M() { }
    void N() { M( ); }
}";

            var test = new CSharpCodeFixTest<GM0092Analyzer, GM0092CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0092.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0092.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task HasSpace_ObjectCreation_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    void M() { var f = new Foo{|GM0092:(|} ); }
}";
            var fixedCode = @"class Foo
{
    void M() { var f = new Foo(); }
}";

            var test = new CSharpCodeFixTest<GM0092Analyzer, GM0092CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
