namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0065Analyzer>;

    public class GM0065AnalyzerTests
    {
        [Fact]
        public async Task Return_SoleStatement_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    bool M()
    {
        return true;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_WithBlankLineBefore_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    bool M()
    {
        bool isUnlocked = true;

        return isUnlocked;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_WithoutBlankLineBefore_Diagnostic()
        {
            var testCode = @"class Foo
{
    bool M()
    {
        bool isUnlocked = true;
        {|GM0065:return isUnlocked;|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_WithMultipleBlankLinesBefore_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    bool M()
    {
        bool isUnlocked = true;


        return isUnlocked;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_AfterBlockStatement_WithTwoBlankLines_NoDiagnostic()
        {
            var testCode = @"using System.Text.RegularExpressions;

class Foo
{
    string M(string input)
    {
        string result = input;

        if (!string.IsNullOrEmpty(input))
        {
            result = Regex.Replace(input, @""[\s\-]+"", ""_"");
            result = Regex.Replace(result, @""[^\w]"", """");
            result = Regex.Replace(result, @""_+"", ""_"");
            result = result.Trim('_');
            result = result.ToUpperInvariant();
        }


        return result;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_FirstStatementInBlock_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    bool M(bool condition)
    {
        if (condition)
        {
            return true;
        }

        return false;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_InNestedBlock_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    bool M(bool condition)
    {
        if (condition)
        {
            bool isUnlocked = true;
            {|GM0065:return isUnlocked;|}
        }

        return false;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_MultipleStatements_WithBlankLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M()
    {
        int x = 1;
        int y = 2;

        return x + y;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_MultipleStatements_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M()
    {
        int x = 1;
        int y = 2;
        {|GM0065:return x + y;|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
