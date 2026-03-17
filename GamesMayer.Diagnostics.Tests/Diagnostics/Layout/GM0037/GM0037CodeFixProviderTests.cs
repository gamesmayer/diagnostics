namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0037CodeFixProviderTests
    {
        [Fact]
        public async Task TypeAndIdentifierOnDifferentLines_Fix()
        {
            var testCode = @"class C
{
    void M({|GM0037:int
        value|})
    {
    }
}";
            var fixedCode = @"class C
{
    void M(int value)
    {
    }
}";

            var test = new CSharpCodeFixTest<GM0037Analyzer, GM0037CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task IdentifierAndEqualsOnDifferentLines_Fix()
        {
            var testCode = @"class C
{
    void M({|GM0037:int value
        = 1|})
    {
    }
}";
            var fixedCode = @"class C
{
    void M(int value = 1)
    {
    }
}";

            var test = new CSharpCodeFixTest<GM0037Analyzer, GM0037CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ModifierAndTypeOnDifferentLines_Fix()
        {
            var testCode = @"class C
{
    void M({|GM0037:in
        int value|})
    {
    }
}";
            var fixedCode = @"class C
{
    void M(in int value)
    {
    }
}";

            var test = new CSharpCodeFixTest<GM0037Analyzer, GM0037CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
