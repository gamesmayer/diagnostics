namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0111CodeFixProviderTests
    {
        [Fact]
        public async Task SpaceBeforeSemicolon_RemovesGap()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0 {|GM0111:;|}
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        int x = 0;
    }
}";

            var test = new CSharpCodeFixTest<GM0111Analyzer, GM0111CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task LineBreakBeforeSemicolon_RemovesGap()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0
{|GM0111:;|}
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        int x = 0;
    }
}";

            var test = new CSharpCodeFixTest<GM0111Analyzer, GM0111CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBeforeFieldSemicolon_RemovesGap()
        {
            var testCode = @"class Foo
{
    private int _value {|GM0111:;|}
}";
            var fixedCode = @"class Foo
{
    private int _value;
}";

            var test = new CSharpCodeFixTest<GM0111Analyzer, GM0111CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
