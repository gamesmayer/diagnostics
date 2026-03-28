namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0066CodeFixProviderTests
    {
        [Fact]
        public async Task IfStatement_MissingBlankLine_AddsBlankLine()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        int x = 1;
        {|GM0066:if|} (condition)
        {
            System.Console.WriteLine(x);
        }
    }
}";

            var fixedCode = @"class Foo
{
    void M(bool condition)
    {
        int x = 1;

        if (condition)
        {
            System.Console.WriteLine(x);
        }
    }
}";

            var test = new CSharpCodeFixTest<GM0066Analyzer, GM0066CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ForStatement_MissingBlankLine_AddsBlankLine()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        {|GM0066:for|} (int i = 0; i < 10; i++)
        {
            x++;
        }
    }
}";

            var fixedCode = @"class Foo
{
    void M()
    {
        int x = 0;

        for (int i = 0; i < 10; i++)
        {
            x++;
        }
    }
}";

            var test = new CSharpCodeFixTest<GM0066Analyzer, GM0066CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task TryStatement_MissingBlankLine_AddsBlankLine()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        {|GM0066:try|}
        {
            x++;
        }
        catch (System.Exception)
        {
        }
    }
}";

            var fixedCode = @"class Foo
{
    void M()
    {
        int x = 0;

        try
        {
            x++;
        }
        catch (System.Exception)
        {
        }
    }
}";

            var test = new CSharpCodeFixTest<GM0066Analyzer, GM0066CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleViolations_BatchFixed()
        {
            var testCode = @"class Foo
{
    void M(bool a, bool b)
    {
        int x = 0;
        {|GM0066:if|} (a)
        {
            x++;
        }
        {|GM0066:if|} (b)
        {
            x--;
        }
    }
}";

            var fixedCode = @"class Foo
{
    void M(bool a, bool b)
    {
        int x = 0;

        if (a)
        {
            x++;
        }

        if (b)
        {
            x--;
        }
    }
}";

            var test = new CSharpCodeFixTest<GM0066Analyzer, GM0066CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
