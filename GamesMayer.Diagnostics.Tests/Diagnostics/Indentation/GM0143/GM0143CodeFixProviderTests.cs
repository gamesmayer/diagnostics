namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0143CodeFixProviderTests
    {
        [Fact]
        public async Task CatchOnSameLineAsClosingBrace_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        } {|GM0143:catch|} (System.Exception)
        {
        }
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        try
        {
        }
        catch (System.Exception)
        {
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0143Analyzer, GM0143CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FinallyOnSameLineAsClosingBrace_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        } {|GM0143:finally|}
        {
        }
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        try
        {
        }
        finally
        {
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0143Analyzer, GM0143CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ElseOnSameLineAsClosingBrace_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        } {|GM0143:else|}
        {
        }
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        if (true)
        {
        }
        else
        {
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0143Analyzer, GM0143CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ElseIfOnSameLineAsClosingBrace_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        } {|GM0143:else|} if (false)
        {
        }
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        if (true)
        {
        }
        else if (false)
        {
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0143Analyzer, GM0143CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
