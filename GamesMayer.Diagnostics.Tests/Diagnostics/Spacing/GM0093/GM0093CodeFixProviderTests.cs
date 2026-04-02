namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0093CodeFixProviderTests
    {
        [Fact]
        public async Task HasSpace_Invocation_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M {|GM0093:(|}value);
    }
}";
            var fixedCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M(value);
    }
}";

            var test = new CSharpCodeFixTest<GM0093Analyzer, GM0093CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_Invocation_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M{|GM0093:(|}value);
    }
}";
            var fixedCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M (value);
    }
}";

            var test = new CSharpCodeFixTest<GM0093Analyzer, GM0093CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0093.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0093.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task HasSpace_EmptyInvocation_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    int M() => 1;

    int N()
    {
        return M {|GM0093:(|});
    }
}";
            var fixedCode = @"class Foo
{
    int M() => 1;

    int N()
    {
        return M();
    }
}";

            var test = new CSharpCodeFixTest<GM0093Analyzer, GM0093CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task HasSpace_ObjectCreation_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var value = new Foo {|GM0093:(|});
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        var value = new Foo();
    }
}";

            var test = new CSharpCodeFixTest<GM0093Analyzer, GM0093CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_ObjectCreation_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var value = new Foo{|GM0093:(|});
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        var value = new Foo ();
    }
}";

            var test = new CSharpCodeFixTest<GM0093Analyzer, GM0093CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0093.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0093.enabled = true"));

            await test.RunAsync();
        }
    }
}
