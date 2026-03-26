namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0051CodeFixProviderTests
    {
        [Fact]
        public async Task Return_ExpressionOnNextLine_Fix()
        {
            var testCode = @"class C
{
    int M()
    {
        return
            {|GM0051:42|};
    }
}";
            var fixedCode = @"class C
{
    int M()
    {
        return 42;
    }
}";
            var test = new CSharpCodeFixTest<GM0051Analyzer, GM0051CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Return_ComplexExpressionOnNextLine_Fix()
        {
            var testCode = @"class C
{
    string[] items = new string[0];

    string M()
    {
        return
            {|GM0051:items[0]|};
    }
}";
            var fixedCode = @"class C
{
    string[] items = new string[0];

    string M()
    {
        return items[0];
    }
}";
            var test = new CSharpCodeFixTest<GM0051Analyzer, GM0051CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
