namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0015Analyzer>;

    public class GM0015AnalyzerTests
    {
        [Fact]
        public async Task IfElse_NoBlankLine_NoDiagnostic()
        {
            var testCode = @"class Foo
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfElse_BlankLineBetween_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfElseIf_BlankLineBetween_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
        }
{|GM0015:|}
        else if (false)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfElseIfElse_MultipleBlankLines_AllFlagged()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
        }
{|GM0015:|}
        else if (false)
        {
        }
{|GM0015:|}
        else
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Switch_NoBlankLineBetweenCases_NoDiagnostic()
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
            case 2:
            {
                break;
            }
            default:
            {
                break;
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Switch_BlankLineBetweenCases_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Switch_BlankLineBeforeDefault_Diagnostic()
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
            default:
            {
                break;
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TryCatch_NoBlankLine_NoDiagnostic()
        {
            var testCode = @"class Foo
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TryCatch_BlankLineBetween_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TryCatchFinally_BlankLineBeforeFinally_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        try
        {
        }
        catch
        {
        }
{|GM0015:|}
        finally
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TryCatchFinally_BlankLinesBeforeBothCatchAndFinally_AllFlagged()
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
{|GM0015:|}
        finally
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenElse_AllFlagged()
        {
            var testCode = "class Foo\n{\n    void Method()\n    {\n        if (true)\n        {\n        }\n{|GM0015:|}\n{|GM0015:|}\n        else\n        {\n        }\n    }\n}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
