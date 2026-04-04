namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0020Analyzer>;

    public class GM0020CodeFixProviderTests
    {
        [Fact]
        public async Task NonEmptyMethodBraceOnSameLine_Fix()
        {
            var testCode = @"class Foo
{
    void Method() {|GM0020:{|}
        int value = 1;
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int value = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0020Analyzer, GM0020CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NonEmptyTypeBraceOnSameLine_Fix()
        {
            var testCode = @"class Foo {|GM0020:{|}
    int value;
}";
            var fixedCode = @"class Foo
{
    int value;
}";
            var test = new CSharpCodeFixTest<GM0020Analyzer, GM0020CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NonEmptyMethodClosingBraceOnSameLine_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int value = 1; }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int value = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0020Analyzer, GM0020CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic().WithSpan(5, 24, 5, 25).WithArguments("closing", "on a new line"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ControlBlockBraceOnSameLine_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true) {|GM0020:{|}
            int value = 1;
        }
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
            int value = 1;
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0020Analyzer, GM0020CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NonEmptyMethodBraceOnNewLine_WithKAndRStyle_Fix()
        {
            var testCode = @"class Foo {
    void Method()
    {|GM0020:{|}
        int value = 1;
    }
}";
            var fixedCode = @"class Foo {
    void Method() {
        int value = 1;
    }
}";
            var test = CreateCodeFixTest(
                testCode,
                fixedCode,
                @"dotnet_diagnostic.GM0020.style = K&R");

            await test.RunAsync();
        }

        [Fact]
        public async Task NonEmptyTypeBraceOnNewLine_WithKAndRStyle_Fix()
        {
            var testCode = @"class Foo
{|GM0020:{|}
    int value;
}";
            var fixedCode = @"class Foo {
    int value;
}";
            var test = CreateCodeFixTest(
                testCode,
                fixedCode,
                @"dotnet_diagnostic.GM0020.style = K&R");

            await test.RunAsync();
        }

        [Fact]
        public async Task ControlBlockBraceOnNewLine_WithKAndRStyle_Fix()
        {
            var testCode = @"class Foo {
    void Method() {
        if (true)
        {|GM0020:{|}
            int value = 1;
        }
    }
}";
            var fixedCode = @"class Foo {
    void Method() {
        if (true) {
            int value = 1;
        }
    }
}";
            var test = CreateCodeFixTest(
                testCode,
                fixedCode,
                @"dotnet_diagnostic.GM0020.style = K&R");

            await test.RunAsync();
        }

        [Fact]
        public async Task NonEmptyMethodClosingBraceOnSameLine_WithKAndRStyle_Fix()
        {
            var testCode = @"class Foo {
    void Method() {
        int value = 1; }
}";
            var fixedCode = @"class Foo {
    void Method() {
        int value = 1;
    }
}";
            var test = CreateCodeFixTest(
                testCode,
                fixedCode,
                @"dotnet_diagnostic.GM0020.style = K&R");
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic().WithSpan(3, 24, 3, 25).WithArguments("closing", "on a new line"));

            await test.RunAsync();
        }

        private static CSharpCodeFixTest<GM0020Analyzer, GM0020CodeFixProvider, XUnitVerifier> CreateCodeFixTest(
            string testCode,
            string fixedCode,
            string editorConfigBody)
        {
            var test = new CSharpCodeFixTest<GM0020Analyzer, GM0020CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", CreateEditorConfig(editorConfigBody)));
            return test;
        }

        private static string CreateEditorConfig(string body)
        {
            return $@"root = true

[*.cs]
{body}";
        }
    }
}
