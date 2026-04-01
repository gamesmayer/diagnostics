namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0080Analyzer>;

    public class GM0080AnalyzerTests
    {
        [Fact]
        public async Task Break_SoleStatement_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            {
                break;
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Break_WithBlankLineBefore_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            {
                int x = 1;

                break;
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Break_WithoutBlankLineBefore_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            {
                int x = 1;
                {|GM0080:break;|}
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Break_WithMultipleBlankLinesBefore_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            {
                int x = 1;


                break;
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Break_FirstStatementInBlock_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            {
                break;
            }
            case 2:
            {
                int x = 1;

                break;
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Break_InNestedBlock_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            {
                if (true)
                {
                    int x = 1;
                    {|GM0080:break;|}
                }
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Break_MultipleStatements_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
            {
                int x = 1;
                int y = 2;
                {|GM0080:break;|}
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
