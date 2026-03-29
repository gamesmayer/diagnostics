namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0075CodeFixProviderTests
    {
        [Fact]
        public async Task EndIfDirective_BlankLineBefore_Removed()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0075:|}
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

            var test = new CSharpCodeFixTest<GM0075Analyzer, GM0075CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ElseDirective_BlankLineBefore_Removed()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0075:|}
#else
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

            var test = new CSharpCodeFixTest<GM0075Analyzer, GM0075CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ElifDirective_BlankLineBefore_Removed()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0075:|}
#elif false
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

            var test = new CSharpCodeFixTest<GM0075Analyzer, GM0075CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
