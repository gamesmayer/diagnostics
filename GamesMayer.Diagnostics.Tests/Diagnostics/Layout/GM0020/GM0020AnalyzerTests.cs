namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0020Analyzer>;

    public class GM0020AnalyzerTests
    {
        [Fact]
        public async Task NonEmptyMethodBraceOnNewLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int value = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonEmptyMethodBraceOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method() {|GM0020:{|}
        int value = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyMethodBraceOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonEmptyMethodClosingBraceOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int value = 1; }
}";

            await VerifyCS.VerifyAnalyzerAsync(
                testCode,
                VerifyCS.Diagnostic().WithSpan(5, 24, 5, 25).WithArguments("closing", "on a new line"));
        }

        [Fact]
        public async Task NonEmptyTypeBraceOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo {|GM0020:{|}
    int value;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyTypeBraceOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ControlBlockBraceOnSameLine_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ControlBlockBraceOnNewLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
            int value = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyControlBlockBraceOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CategoriesConfiguration_PropertiesSkipped_IndexersAndEventsAnalyzed()
        {
            var testCode = @"class Foo
{
    int Value {
        get => 1;
    }

    int this[int i] {|GM0020:{|}
        get => i;
    }

    event System.Action Changed {|GM0020:{|}
        add { }
        remove { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0020Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0020 = accessors, anonymous_methods, anonymous_types, control_blocks, events, indexers, lambdas, local_functions, methods, object_collection_array_initializers, types"));

            await test.RunAsync();
        }

        [Fact]
        public async Task AutoImplementedProperty_BraceOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public int Value { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AutoImplementedProperty_GetOnly_BraceOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public int Value { get; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchCaseBlockBraceOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1: {|GM0020:{|}
                int value = 1;
                break;
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchCaseBlockBraceOnNewLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
            {
                int value = 1;
                break;
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonAutoImplementedProperty_BraceOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    public int Value {|GM0020:{|}
        get
        {
            return 0;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExplicitAllmanStyle_NonEmptyMethodBraceOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method() {|GM0020:{|}
        int value = 1;
    }
}";

            var test = CreateAnalyzerTest(testCode, @"dotnet_diagnostic.GM0020.style = Allman");

            await test.RunAsync();
        }

        [Fact]
        public async Task KAndRStyle_NonEmptyMethodBraceOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo {
    void Method() {
        int value = 1;
    }
}";

            var test = CreateAnalyzerTest(testCode, @"dotnet_diagnostic.GM0020.style = K&R");

            await test.RunAsync();
        }

        [Fact]
        public async Task KAndRStyle_NonEmptyMethodClosingBraceOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo {
    void Method() {
        int value = 1; }
}";

            var test = CreateAnalyzerTest(testCode, @"dotnet_diagnostic.GM0020.style = K&R");
            test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic().WithSpan(3, 24, 3, 25).WithArguments("closing", "on a new line"));

            await test.RunAsync();
        }

        [Fact]
        public async Task KAndRStyle_NonEmptyMethodBraceOnNewLine_Diagnostic()
        {
            var testCode = @"class Foo {
    void Method()
    {|GM0020:{|}
        int value = 1;
    }
}";

            var test = CreateAnalyzerTest(testCode, @"dotnet_diagnostic.GM0020.style = K&R");

            await test.RunAsync();
        }

        [Fact]
        public async Task InvalidStyle_FallsBackToAllman()
        {
            var testCode = @"class Foo
{
    void Method() {|GM0020:{|}
        int value = 1;
    }
}";

            var test = CreateAnalyzerTest(testCode, @"dotnet_diagnostic.GM0020.style = Stroustrup");

            await test.RunAsync();
        }

        [Fact]
        public async Task CategoriesAndKAndRStyle_ComposeCorrectly()
        {
            var testCode = @"class Foo
{
    int Value
    {
        get
        {
            return 1;
        }
    }

    void Method()
    {|GM0020:{|}
        int value = 1;
    }
}";

            var test = CreateAnalyzerTest(
                testCode,
                @"dotnet_diagnostic.GM0020 = methods
dotnet_diagnostic.GM0020.style = K&R");

            await test.RunAsync();
        }

        private static CSharpAnalyzerTest<GM0020Analyzer, XUnitVerifier> CreateAnalyzerTest(
            string testCode,
            string editorConfigBody)
        {
            var test = new CSharpAnalyzerTest<GM0020Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
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
