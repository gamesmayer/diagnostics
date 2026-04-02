namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0024CodeFixProviderTests
    {
        [Fact]
        public async Task MisindentedOpeningBrace_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
            {|GM0024:{|}
            return;
        };
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
            return;
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0024Analyzer, GM0024CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MisindentedStatement_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
        {|GM0024:return|};
        };
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
            return;
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0024Analyzer, GM0024CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MisindentedClosingBrace_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
            return;
            {|GM0024:}|};
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
            return;
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0024Analyzer, GM0024CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MisindentedOpeningBrace_AnonymousMethod_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = delegate
            {|GM0024:{|}
            return;
        };
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        System.Action action = delegate
        {
            return;
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0024Analyzer, GM0024CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
