namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0036CodeFixProviderTests
    {
        [Fact]
        public async Task TypeAndIdentifierOnDifferentLines_Fix()
        {
            var testCode = @"using System.Linq;

class C
{
    void M(int[] numbers)
    {
        {|GM0036:var
        value = numbers
            .Where(x => x > 0)
            .Select(x => x * 2)
            .ToArray();|}
    }
}";
            var fixedCode = @"using System.Linq;

class C
{
    void M(int[] numbers)
    {
        var value = numbers
            .Where(x => x > 0)
            .Select(x => x * 2)
            .ToArray();
    }
}";

            var test = new CSharpCodeFixTest<GM0036Analyzer, GM0036CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task DeclarationWithoutInitializerOnDifferentLines_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        {|GM0036:int
        value;|}
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        int value;
    }
}";

            var test = new CSharpCodeFixTest<GM0036Analyzer, GM0036CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
