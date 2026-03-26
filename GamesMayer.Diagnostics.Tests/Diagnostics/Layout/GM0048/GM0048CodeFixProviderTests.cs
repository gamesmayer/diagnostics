namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0048CodeFixProviderTests
    {
        [Fact]
        public async Task SimpleLambda_BodyOnNextLine_Fix()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        Func<int, int> f = x =>
            {|GM0048:x + 1|};
    }
}";
            var fixedCode = @"using System;
class C
{
    void M()
    {
        Func<int, int> f = x => x + 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0048Analyzer, GM0048CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ParenthesizedLambda_BodyOnNextLine_Fix()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        Func<int, int, int> f = (x, y) =>
            {|GM0048:x + y|};
    }
}";
            var fixedCode = @"using System;
class C
{
    void M()
    {
        Func<int, int, int> f = (x, y) => x + y;
    }
}";
            var test = new CSharpCodeFixTest<GM0048Analyzer, GM0048CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ExpressionBodiedProperty_BodyOnNextLine_Fix()
        {
            var testCode = @"class C
{
    int GetValue() =>
        {|GM0048:42|};
}";
            var fixedCode = @"class C
{
    int GetValue() => 42;
}";
            var test = new CSharpCodeFixTest<GM0048Analyzer, GM0048CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
