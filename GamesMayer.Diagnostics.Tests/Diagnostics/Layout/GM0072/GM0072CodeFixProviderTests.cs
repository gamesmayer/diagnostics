namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0072CodeFixProviderTests
    {
        [Fact]
        public async Task IfDirective_MissingBlankLine_AddsBlankLine()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 1;
{|GM0072:#if|} true
        System.Console.WriteLine(x);
#endif
    }
}";

            var fixedCode = @"class Foo
{
    void M()
    {
        int x = 1;

#if true
        System.Console.WriteLine(x);
#endif
    }
}";

            var test = new CSharpCodeFixTest<GM0072Analyzer, GM0072CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task IfDirective_InsideBlock_MissingBlankLine_AddsBlankLine()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
            int x = 1;
{|GM0072:#if|} true
            System.Console.WriteLine(x);
#endif
        }
    }
}";

            var fixedCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
            int x = 1;

#if true
            System.Console.WriteLine(x);
#endif
        }
    }
}";

            var test = new CSharpCodeFixTest<GM0072Analyzer, GM0072CodeFixProvider, XUnitVerifier>
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
        int x = 1;
{|GM0072:#if|} true
        System.Console.WriteLine(x);
#endif
        int y = 2;
{|GM0072:#if|} false
        System.Console.WriteLine(y);
#endif
    }
}";

            var fixedCode = @"class Foo
{
    void M()
    {
        int x = 1;

#if true
        System.Console.WriteLine(x);
#endif
        int y = 2;

#if false
        System.Console.WriteLine(y);
#endif
    }
}";

            var test = new CSharpCodeFixTest<GM0072Analyzer, GM0072CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
