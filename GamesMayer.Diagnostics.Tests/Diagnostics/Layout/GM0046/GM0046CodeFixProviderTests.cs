namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0046CodeFixProviderTests
    {
        [Fact]
        public async Task AndOperator_AtStartOfNextLine_Fix()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null
            {|GM0046:&&|} a.Length > 0;
    }
}";
            var fixedCode = @"class C
{
    void M(string a)
    {
        bool b = a != null &&
            a.Length > 0;
    }
}";
            var test = new CSharpCodeFixTest<GM0046Analyzer, GM0046CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task OperatorOnOwnLine_Fix()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null
            {|GM0046:&&|}
            a.Length > 0;
    }
}";
            var fixedCode = @"class C
{
    void M(string a)
    {
        bool b = a != null &&
            a.Length > 0;
    }
}";
            var test = new CSharpCodeFixTest<GM0046Analyzer, GM0046CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ArithmeticOperator_AtStartOfNextLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        int x = 0 +
            1 -
            2
            {|GM0046:*|} 3;
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        int x = 0 +
            1 -
            2 *
            3;
    }
}";
            var test = new CSharpCodeFixTest<GM0046Analyzer, GM0046CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ThreeOperands_AllViolations_Fix()
        {
            var testCode = @"class C
{
    void M(string a, string b)
    {
        bool result = a != null
            {|GM0046:&&|} a.Length > 0
            {|GM0046:&&|} b != null;
    }
}";
            var fixedCode = @"class C
{
    void M(string a, string b)
    {
        bool result = a != null &&
            a.Length > 0 &&
            b != null;
    }
}";
            var test = new CSharpCodeFixTest<GM0046Analyzer, GM0046CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfIncrementalIterations = 2,
            };
            await test.RunAsync();
        }
    }
}
