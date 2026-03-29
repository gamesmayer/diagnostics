namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0073CodeFixProviderTests
    {
        [Fact]
        public async Task EndIfDirective_MissingBlankLine_AddsBlankLine()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0073:#endif|}
        int x = 1;
    }
}";

            var fixedCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#endif

        int x = 1;
    }
}";

            var test = new CSharpCodeFixTest<GM0073Analyzer, GM0073CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EndIfDirective_InsideBlock_MissingBlankLine_AddsBlankLine()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
#if true
            System.Console.WriteLine();
{|GM0073:#endif|}
            int x = 1;
        }
    }
}";

            var fixedCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
#if true
            System.Console.WriteLine();
#endif

            int x = 1;
        }
    }
}";

            var test = new CSharpCodeFixTest<GM0073Analyzer, GM0073CodeFixProvider, XUnitVerifier>
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
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0073:#endif|}
        int x = 1;

#if false
        System.Console.WriteLine();
{|GM0073:#endif|}
        int y = 2;
    }
}";

            var fixedCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#endif

        int x = 1;

#if false
        System.Console.WriteLine();
#endif

        int y = 2;
    }
}";

            var test = new CSharpCodeFixTest<GM0073Analyzer, GM0073CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
