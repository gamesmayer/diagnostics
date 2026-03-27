namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0055Analyzer>;

    public class GM0055AnalyzerTests
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
        public async Task MethodWithSingleConstraint_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithTwoContiguousConstraints_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
        where U : System.ICloneable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithBlankLineBetweenConstraints_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
{|GM0055:
|}        where U : System.ICloneable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithMultipleBlankLinesBetweenConstraints_MultipleDiagnostics()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
{|GM0055:
|}{|GM0055:
|}        where U : System.ICloneable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithThreeConstraintsAndBlankLines_MultipleDiagnostics()
        {
            var testCode = @"class Foo
{
    public void Method<T, U, V>()
        where T : System.IDisposable
{|GM0055:
|}        where U : System.ICloneable
{|GM0055:
|}        where V : System.IComparable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithBlankLineBetweenConstraints_Diagnostic()
        {
            var testCode = @"public class Foo<T, U>
    where T : System.IDisposable
{|GM0055:
|}    where U : System.ICloneable
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithNonBlankLineBetweenConstraints_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
        // comment
        where U : System.ICloneable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
