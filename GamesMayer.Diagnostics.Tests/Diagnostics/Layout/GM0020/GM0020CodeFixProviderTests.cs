namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

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
    }
}
