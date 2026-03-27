namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0057Analyzer>;

    public class GM0057AnalyzerTests
    {
        [Fact]
        public async Task MethodWithNoConstraints_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithConstraintOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>() where T : System.IDisposable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithConstraintOnNextLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithBlankLineBetweenDeclarationAndFirstConstraint_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
{|GM0057:
|}        where T : System.IDisposable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithMultipleBlankLinesBetweenDeclarationAndFirstConstraint_MultipleDiagnostics()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
{|GM0057:
|}{|GM0057:
|}        where T : System.IDisposable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithBlankLineOnlyBetweenDeclarationAndFirst_NotBetweenSubsequent_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
{|GM0057:
|}        where T : System.IDisposable
        where U : System.ICloneable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithConstraintOnNextLine_NoDiagnostic()
        {
            var testCode = @"public class Foo<T>
    where T : System.IDisposable
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithBlankLineBetweenDeclarationAndFirstConstraint_Diagnostic()
        {
            var testCode = @"public class Foo<T>
{|GM0057:
|}    where T : System.IDisposable
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithNonBlankLineBetweenDeclarationAndFirstConstraint_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
        // comment
        where T : System.IDisposable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
