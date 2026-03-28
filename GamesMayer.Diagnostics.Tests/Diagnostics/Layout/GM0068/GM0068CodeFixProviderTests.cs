namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0068CodeFixProviderTests
    {
        [Fact]
        public async Task TwoStatements_SameLine_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int i = 0; {|GM0068:System.Console.WriteLine(i);|}
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        int i = 0;
        System.Console.WriteLine(i);
    }
}";
            var test = new CSharpCodeFixTest<GM0068Analyzer, GM0068CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ThreeStatements_TwoOnSameLine_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int i = 0; {|GM0068:int j = 1;|}
        System.Console.WriteLine(i + j);
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        int i = 0;
        int j = 1;
        System.Console.WriteLine(i + j);
    }
}";
            var test = new CSharpCodeFixTest<GM0068Analyzer, GM0068CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
