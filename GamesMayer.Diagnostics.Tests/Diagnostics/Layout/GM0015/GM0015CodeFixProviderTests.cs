namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0015CodeFixProviderTests
    {
        [Fact]
        public async Task IfElse_BlankLineBetween_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
        }
{|GM0015:|}
        else
        {
        }
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
        }
        else
        {
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0015Analyzer, GM0015CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Switch_BlankLineBetweenCases_Fix()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
            {
                break;
            }
{|GM0015:|}
            case 2:
            {
                break;
            }
        }
    }
}";
            var fixedCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
            {
                break;
            }
            case 2:
            {
                break;
            }
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0015Analyzer, GM0015CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task TryCatch_BlankLineBetween_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        try
        {
        }
{|GM0015:|}
        catch
        {
        }
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        try
        {
        }
        catch
        {
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0015Analyzer, GM0015CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenElse_Fix()
        {
            var testCode = "class Foo\n{\n    void Method()\n    {\n        if (true)\n        {\n        }\n{|GM0015:|}\n{|GM0015:|}\n        else\n        {\n        }\n    }\n}";
            var fixedCode = "class Foo\n{\n    void Method()\n    {\n        if (true)\n        {\n        }\n        else\n        {\n        }\n    }\n}";
            var test = new CSharpCodeFixTest<GM0015Analyzer, GM0015CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfIncrementalIterations = 2,
            };
            await test.RunAsync();
        }
    }
}
