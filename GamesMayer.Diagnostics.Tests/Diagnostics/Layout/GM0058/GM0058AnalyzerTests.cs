namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0058Analyzer>;

    public class GM0058AnalyzerTests
    {
        [Fact]
        public async Task ClassWithNoBaseList_NoDiagnostic()
        {
            var testCode = @"class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithBaseTypeOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo : System.IDisposable
{
    public void Dispose() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithMultipleBaseTypesOnSameLine_NoDiagnostic()
        {
            var testCode = @"interface IBar { }
class Foo : System.IDisposable, IBar
{
    public void Dispose() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithColonOnNextLine_Diagnostic()
        {
            var testCode = @"class Foo
    {|GM0058:: System.IDisposable|}
{
    public void Dispose() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithColonAndBaseTypeOnSeparateLines_Diagnostic()
        {
            var testCode = @"class Foo
    {|GM0058::
    System.IDisposable|}
{
    public void Dispose() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithBaseTypesOnDifferentLines_Diagnostic()
        {
            var testCode = @"interface IBar { }
class Foo
    {|GM0058:: System.IDisposable,
    IBar|}
{
    public void Dispose() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InterfaceWithBaseTypeOnSameLine_NoDiagnostic()
        {
            var testCode = @"interface IFoo : System.IDisposable { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InterfaceWithColonOnNextLine_Diagnostic()
        {
            var testCode = @"interface IFoo
    {|GM0058:: System.IDisposable|} { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
