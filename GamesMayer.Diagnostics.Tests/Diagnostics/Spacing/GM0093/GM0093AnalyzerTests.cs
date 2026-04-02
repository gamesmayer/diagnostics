namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0093Analyzer>;

    public class GM0093AnalyzerTests
    {
        [Fact]
        public async Task NoSpace_Invocation_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M(value);
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task HasSpace_Invocation_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M {|GM0093:(|}value);
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpace_Invocation_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M{|GM0093:(|}value);
    }
}";

            var test = new CSharpAnalyzerTest<GM0093Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0093.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task HasSpace_Invocation_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M (value);
    }
}";

            var test = new CSharpAnalyzerTest<GM0093Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0093.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task HasSpace_EmptyInvocation_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M() => 1;

    int N()
    {
        return M {|GM0093:(|});
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpace_EmptyInvocation_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M() => 1;

    int N ()
    {
        return M{|GM0093:(|});
    }
}";

            var test = new CSharpAnalyzerTest<GM0093Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0093.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_ObjectCreation_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var value = new Foo();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task HasSpace_ObjectCreation_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var value = new Foo {|GM0093:(|});
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpace_ObjectCreation_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M ()
    {
        var value = new Foo{|GM0093:(|});
    }
}";

            var test = new CSharpAnalyzerTest<GM0093Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0093.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task HasSpace_ObjectCreation_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M ()
    {
        var value = new Foo ();
    }
}";

            var test = new CSharpAnalyzerTest<GM0093Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0093.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_ImplicitObjectCreation_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        Foo value = new();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task HasSpace_ImplicitObjectCreation_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        Foo value = new {|GM0093:(|});
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpace_ImplicitObjectCreation_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        Foo value = new{|GM0093:(|});
    }
}";

            var test = new CSharpAnalyzerTest<GM0093Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0093.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task HasSpace_ImplicitObjectCreation_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        Foo value = new ();
    }
}";

            var test = new CSharpAnalyzerTest<GM0093Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0093.enabled = true"));

            await test.RunAsync();
        }
    }
}
