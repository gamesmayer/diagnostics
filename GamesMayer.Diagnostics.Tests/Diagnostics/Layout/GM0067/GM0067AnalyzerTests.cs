namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0067Analyzer>;

    public class GM0067AnalyzerTests
    {
        [Fact]
        public async Task IfStatement_LastInBlock_NoDiagnostic()
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
        public async Task IfStatement_NotLast_WithBlankLine_NoDiagnostic()
        {
            var testCode = @"class Foo
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfStatement_NotLast_WithoutBlankLine_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfElse_NotLast_WithoutBlankLine_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForStatement_NotLast_WithoutBlankLine_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForeachStatement_NotLast_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int[] items)
    {
        foreach (int item in items)
        {
            System.Console.WriteLine(item);
        {|GM0067:}|}
        System.Console.WriteLine(""done"");
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WhileStatement_NotLast_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        while (x < 10)
        {
            x++;
        {|GM0067:}|}
        System.Console.WriteLine(x);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DoStatement_NotLast_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        do
        {
            x++;
        }
        while (x < 10){|GM0067:;|}
        System.Console.WriteLine(x);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchStatement_NotLast_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int x)
    {
        switch (x)
        {
            default:
                break;
        {|GM0067:}|}
        System.Console.WriteLine(x);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TryStatement_NotLast_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        try
        {
            x++;
        }
        catch (System.Exception)
        {
        {|GM0067:}|}
        System.Console.WriteLine(x);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LockStatement_NotLast_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    private readonly object _lock = new object();

    void M()
    {
        int x = 0;
        lock (_lock)
        {
            x++;
        {|GM0067:}|}
        System.Console.WriteLine(x);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UsingStatement_NotLast_WithoutBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 0;
        using (var r = new System.IO.MemoryStream())
        {
            x++;
        {|GM0067:}|}
        System.Console.WriteLine(x);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForStatement_LastInBlock_NoDiagnostic()
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
        public async Task NestedIfStatement_LastInInnerBlock_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool a, bool b)
    {
        if (a)
        {
            if (b)
            {
                System.Console.WriteLine();
            }
        }

        System.Console.WriteLine(""done"");
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedIfStatement_NotLastInInnerBlock_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool a, bool b)
    {
        if (a)
        {
            if (b)
            {
                System.Console.WriteLine();
            {|GM0067:}|}
            System.Console.WriteLine(""inner done"");
        }

        System.Console.WriteLine(""done"");
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
