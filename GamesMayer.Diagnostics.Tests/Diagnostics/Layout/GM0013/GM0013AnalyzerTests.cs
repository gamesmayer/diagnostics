namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0013Analyzer>;

    public class GM0013AnalyzerTests
    {
        [Fact]
        public async Task SwitchCase_WithBraces_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
            {
                DoSomething();
                break;
            }
        }
    }

    void DoSomething() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchCase_WithoutBraces_SingleStatement_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
                {|GM0013:break;|}
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchCase_WithoutBraces_MultipleStatements_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
                {|GM0013:DoSomething();
                break;|}
        }
    }

    void DoSomething() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DefaultCase_WithoutBraces_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            default:
                {|GM0013:break;|}
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DefaultCase_WithBraces_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
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
        public async Task SwitchCase_EmptyFallthrough_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
            case 2:
            {
                DoSomething();
                break;
            }
        }
    }

    void DoSomething() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleCases_SomeWithoutBraces_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
            {
                DoSomething();
                break;
            }
            case 2:
                {|GM0013:DoSomething();
                break;|}
        }
    }

    void DoSomething() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
