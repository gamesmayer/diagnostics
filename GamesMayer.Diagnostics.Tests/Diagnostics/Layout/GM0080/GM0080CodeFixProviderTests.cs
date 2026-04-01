namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0080CodeFixProviderTests
    {
        [Fact]
        public async Task Break_MissingBlankLine_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            {
                int x = 1;
                {|GM0080:break;|}
            }
        }
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            {
                int x = 1;

                break;
            }
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0080Analyzer, GM0080CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Break_MissingBlankLine_InNestedBlock_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            {
                if (true)
                {
                    int x = 1;
                    {|GM0080:break;|}
                }
            }
        }
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            {
                if (true)
                {
                    int x = 1;

                    break;
                }
            }
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0080Analyzer, GM0080CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
