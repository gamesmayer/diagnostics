namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0066Analyzer>;

    public class GM0066AnalyzerTests
    {
        [Fact]
        public async Task IfStatement_FirstInBlock_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
            System.Console.WriteLine();
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfStatement_PrecededByBlankLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        int x = 1;

        if (condition)
        {
            System.Console.WriteLine(x);
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfStatement_NotPrecededByBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        int x = 1;
        {|GM0066:if|} (condition)
        {
            System.Console.WriteLine(x);
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForStatement_NotPrecededByBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        {|GM0066:for|} (int i = 0; i < 10; i++)
        {
            x++;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForeachStatement_NotPrecededByBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int[] items)
    {
        int x = 0;
        {|GM0066:foreach|} (int item in items)
        {
            x += item;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WhileStatement_NotPrecededByBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        {|GM0066:while|} (x < 10)
        {
            x++;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DoStatement_NotPrecededByBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        {|GM0066:do|}
        {
            x++;
        }
        while (x < 10);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchStatement_NotPrecededByBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int x)
    {
        int y = x + 1;
        {|GM0066:switch|} (y)
        {
            default:
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TryStatement_NotPrecededByBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        {|GM0066:try|}
        {
            x++;
        }
        catch (System.Exception)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LockStatement_NotPrecededByBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    private readonly object _lock = new object();

    void M()
    {
        int x = 0;
        {|GM0066:lock|} (_lock)
        {
            x++;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UsingStatement_NotPrecededByBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        {|GM0066:using|} (var r = new System.IO.MemoryStream())
        {
            x++;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForStatement_FirstInBlock_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        for (int i = 0; i < 10; i++)
        {
            System.Console.WriteLine(i);
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedIfStatement_FirstInInnerBlock_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool a, bool b)
    {
        int x = 1;

        if (a)
        {
            if (b)
            {
                System.Console.WriteLine(x);
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedIfStatement_NotFirstInInnerBlock_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool a, bool b)
    {
        int x = 1;

        if (a)
        {
            int y = 2;
            {|GM0066:if|} (b)
            {
                System.Console.WriteLine(x + y);
            }
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleViolations_AllReported()
        {
            var testCode = @"class Foo
{
    void M(bool a, bool b)
    {
        int x = 0;
        {|GM0066:if|} (a)
        {
            x++;
        }
        {|GM0066:if|} (b)
        {
            x--;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
