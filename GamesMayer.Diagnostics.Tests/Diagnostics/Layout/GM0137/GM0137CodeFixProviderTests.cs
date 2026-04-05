namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0137CodeFixProviderTests
    {
        [Fact]
        public async Task CastAndOperandOnNextLine_Fix()
        {
            var testCode = @"
class Foo
{
    enum PlayerType { None }

    void Test()
    {
        int index = 0;
        var result = {|GM0137:(PlayerType)
index|};
    }
}
";
            var fixedCode = @"
class Foo
{
    enum PlayerType { None }

    void Test()
    {
        int index = 0;
        var result = (PlayerType)index;
    }
}
";

            var test = new CSharpCodeFixTest<GM0137Analyzer, GM0137CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task CastWithIndentedOperandOnNextLine_Fix()
        {
            var testCode = @"
class Foo
{
    void Test()
    {
        int index = 0;
        var result = {|GM0137:(float)
            index|};
    }
}
";
            var fixedCode = @"
class Foo
{
    void Test()
    {
        int index = 0;
        var result = (float)index;
    }
}
";

            var test = new CSharpCodeFixTest<GM0137Analyzer, GM0137CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task CastAndOperandSeparatedByBlankLine_Fix()
        {
            var testCode = @"
class Foo
{
    enum PlayerType { None }

    void Test()
    {
        int index = 0;
        var result = {|GM0137:(PlayerType)

index|};
    }
}
";
            var fixedCode = @"
class Foo
{
    enum PlayerType { None }

    void Test()
    {
        int index = 0;
        var result = (PlayerType)index;
    }
}
";

            var test = new CSharpCodeFixTest<GM0137Analyzer, GM0137CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
