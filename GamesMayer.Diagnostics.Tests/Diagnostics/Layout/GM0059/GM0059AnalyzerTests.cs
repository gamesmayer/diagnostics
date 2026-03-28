namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0059Analyzer>;

    public class GM0059AnalyzerTests
    {
        [Fact]
        public async Task MethodCallNamedArgumentWithoutSpaceBeforeColon_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Bar(int value) { }

    void M()
    {
        Bar(value: 42);
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorCallNamedArgumentWithoutSpaceBeforeColon_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public Foo(int value) { }

    void M()
    {
        var x = new Foo(value: 42);
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodCallNamedArgumentWithSpaceBeforeColon_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Bar(int value) { }

    void M()
    {
        Bar({|GM0059:value :|} 42);
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorCallNamedArgumentWithLineBreakBeforeColon_Diagnostic()
        {
            var testCode = @"using System.Collections.Generic;

class Foo
{
    void M()
    {
        _ = new List<int>({|GM0059:capacity
            :|} 10);
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
