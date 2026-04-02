namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0054Analyzer>;

    public class GM0054AnalyzerTests
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
        public async Task MethodWithConstraintCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithMultipleConstraintsAllCorrectlyIndented_NoDiagnostic()
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
        public async Task MethodWithConstraintOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>() where T : System.IDisposable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithConstraintNotIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
{|GM0054:where T : System.IDisposable|} { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithConstraintSameIndentAsDeclaration_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
    {|GM0054:where T : System.IDisposable|} { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithConstraintOverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
            {|GM0054:where T : System.IDisposable|} { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithConstraintCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"public class Foo<T>
    where T : System.IDisposable
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithConstraintWronglyIndented_Diagnostic()
        {
            var testCode = @"public class Foo<T>
{|GM0054:where T : System.IDisposable|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithSecondConstraintWronglyIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
{|GM0054:where U : System.ICloneable|} { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
