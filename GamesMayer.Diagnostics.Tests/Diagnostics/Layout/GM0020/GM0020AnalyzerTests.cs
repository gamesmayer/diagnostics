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
    }
}
