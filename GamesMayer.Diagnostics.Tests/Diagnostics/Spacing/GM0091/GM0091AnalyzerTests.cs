namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0091Analyzer>;

    public class GM0091AnalyzerTests
    {
        [Fact]
        public async Task NoSpace_MethodDeclaration_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task HasSpace_MethodDeclaration_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M {|GM0091:(|}int value)
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpace_MethodDeclaration_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M{|GM0091:(|}int value)
    {
    }
}";

            var test = new CSharpAnalyzerTest<GM0091Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0091.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task HasSpace_MethodDeclaration_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M (int value)
    {
    }
}";

            var test = new CSharpAnalyzerTest<GM0091Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0091.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_ConstructorDeclaration_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public Foo()
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task HasSpace_ConstructorDeclaration_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    public Foo {|GM0091:(|})
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpace_ConstructorDeclaration_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    public Foo{|GM0091:(|})
    {
    }
}";

            var test = new CSharpAnalyzerTest<GM0091Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0091.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_DestructorDeclaration_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    ~Foo()
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task HasSpace_DestructorDeclaration_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    ~Foo {|GM0091:(|})
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpace_DestructorDeclaration_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    ~Foo{|GM0091:(|})
    {
    }
}";

            var test = new CSharpAnalyzerTest<GM0091Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0091.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_LocalFunction_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        void Local() { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task HasSpace_LocalFunction_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        void Local {|GM0091:(|}) { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpace_LocalFunction_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M ()
    {
        void Local{|GM0091:(|}) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0091Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0091.enabled = true"));

            await test.RunAsync();
        }
    }
}
