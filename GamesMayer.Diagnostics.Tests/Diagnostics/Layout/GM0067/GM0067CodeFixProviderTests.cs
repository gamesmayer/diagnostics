namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0067CodeFixProviderTests
    {
        [Fact]
        public async Task IfStatement_MissingBlankLine_Fix()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
            System.Console.WriteLine();
        {|GM0067:}|}
        System.Console.WriteLine(""done"");
    }
}";
            var fixedCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
            System.Console.WriteLine();
        }

        System.Console.WriteLine(""done"");
    }
}";
            var test = new CSharpCodeFixTest<GM0067Analyzer, GM0067CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task IfElse_MissingBlankLine_Fix()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
            System.Console.WriteLine(""yes"");
        }
        else
        {
            System.Console.WriteLine(""no"");
        {|GM0067:}|}
        System.Console.WriteLine(""done"");
    }
}";
            var fixedCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
            System.Console.WriteLine(""yes"");
        }
        else
        {
            System.Console.WriteLine(""no"");
        }

        System.Console.WriteLine(""done"");
    }
}";
            var test = new CSharpCodeFixTest<GM0067Analyzer, GM0067CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ForStatement_MissingBlankLine_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {
        for (int i = 0; i < 10; i++)
        {
            System.Console.WriteLine(i);
        {|GM0067:}|}
        System.Console.WriteLine(""done"");
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        for (int i = 0; i < 10; i++)
        {
            System.Console.WriteLine(i);
        }

        System.Console.WriteLine(""done"");
    }
}";
            var test = new CSharpCodeFixTest<GM0067Analyzer, GM0067CodeFixProvider, XUnitVerifier>
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
        if (a)
        {
            System.Console.WriteLine(""a"");
        {|GM0067:}|}
        if (b)
        {
            System.Console.WriteLine(""b"");
        {|GM0067:}|}
        System.Console.WriteLine(""done"");
    }
}";
            var fixedCode = @"class Foo
{
    void M(bool a, bool b)
    {
        if (a)
        {
            System.Console.WriteLine(""a"");
        }

        if (b)
        {
            System.Console.WriteLine(""b"");
        }

        System.Console.WriteLine(""done"");
    }
}";
            var test = new CSharpCodeFixTest<GM0067Analyzer, GM0067CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
