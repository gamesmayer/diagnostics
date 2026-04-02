namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0090CodeFixProviderTests
    {
        [Fact]
        public async Task HasSpace_MethodDeclaration_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    void M{|GM0090:(|} )
    {
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
    }
}";

            var test = new CSharpCodeFixTest<GM0090Analyzer, GM0090CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_MethodDeclaration_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    void M{|GM0090:(|}){
    }
}";
            var fixedCode = @"class Foo
{
    void M( ){
    }
}";

            var test = new CSharpCodeFixTest<GM0090Analyzer, GM0090CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0090.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0090.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task HasSpace_ConstructorDeclaration_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    public Foo{|GM0090:(|} )
    {
    }
}";
            var fixedCode = @"class Foo
{
    public Foo()
    {
    }
}";

            var test = new CSharpCodeFixTest<GM0090Analyzer, GM0090CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task HasSpace_DestructorDeclaration_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    ~Foo{|GM0090:(|} )
    {
    }
}";
            var fixedCode = @"class Foo
{
    ~Foo()
    {
    }
}";

            var test = new CSharpCodeFixTest<GM0090Analyzer, GM0090CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
