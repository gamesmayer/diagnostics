namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0074CodeFixProviderTests
    {
        [Fact]
        public async Task IfDirective_BlankLineAfter_Removed()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
{|GM0074:|}
        System.Console.WriteLine();
#endif
    }
}";

            var fixedCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#endif
    }
}";

            var test = new CSharpCodeFixTest<GM0074Analyzer, GM0074CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ElseDirective_BlankLineAfter_Removed()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#else
{|GM0074:|}
        System.Console.Write();
#endif
    }
}";

            var fixedCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#else
        System.Console.Write();
#endif
    }
}";

            var test = new CSharpCodeFixTest<GM0074Analyzer, GM0074CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ElifDirective_BlankLineAfter_Removed()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#elif false
{|GM0074:|}
        System.Console.Write();
#endif
    }
}";

            var fixedCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#elif false
        System.Console.Write();
#endif
    }
}";

            var test = new CSharpCodeFixTest<GM0074Analyzer, GM0074CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
