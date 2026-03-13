namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0017Analyzer>;

    public class GM0017AnalyzerTests
    {
        [Fact]
        public async Task IfStatement_SingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        bool isTrue = true;
        if (isTrue) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForEachStatement_SingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var items = new int[0];
        foreach (var i in items) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CatchClause_SingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        try { }
        catch (System.Exception ex) { _ = ex; }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CatchClause_NoDeclararation_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        try { }
        catch { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfStatement_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        bool isTrue = true;
        {|GM0017:if (
            isTrue)|}
        { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForEachStatement_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var items = new int[0];
        {|GM0017:foreach (
            var i in items)|}
        { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForStatement_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        {|GM0017:for (
            int i = 0; i < 10; i++)|}
        { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WhileStatement_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        bool isTrue = true;
        {|GM0017:while (
            isTrue)|}
        { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchStatement_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int value)
    {
        {|GM0017:switch (
            value)|}
        { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CatchClause_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        try { }
        {|GM0017:catch (
            System.Exception ex)|}
        { _ = ex; }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LockStatement_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    readonly object _lock = new object();
    void M()
    {
        {|GM0017:lock (
            _lock)|}
        { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task UsingStatement_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        {|GM0017:using (
            var d = new System.IO.MemoryStream())|}
        { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}