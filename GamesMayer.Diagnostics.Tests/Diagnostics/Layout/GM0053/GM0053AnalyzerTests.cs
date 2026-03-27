namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0053Analyzer>;

    public class GM0053AnalyzerTests
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
        public async Task MethodWithConstraintOnOwnLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithMultipleConstraintsEachOnOwnLine_NoDiagnostic()
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
        public async Task MethodWithConstraintOnSameLineAsDeclaration_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>() {|GM0053:where T : System.IDisposable|} { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithTwoConstraintsOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable {|GM0053:where U : System.ICloneable|} { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithAllConstraintsOnSameLine_MultipleDiagnostics()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>() {|GM0053:where T : System.IDisposable|} {|GM0053:where U : System.ICloneable|} { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithConstraintOnOwnLine_NoDiagnostic()
        {
            var testCode = @"public class Foo<T>
    where T : System.IDisposable
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithConstraintOnSameLineAsDeclaration_Diagnostic()
        {
            var testCode = @"public class Foo<T> {|GM0053:where T : System.IDisposable|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithMultipleConstraintsEachOnOwnLine_NoDiagnostic()
        {
            var testCode = @"public class Foo<T, U>
    where T : System.IDisposable
    where U : System.ICloneable
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithMultipleConstraintsOnSameLine_Diagnostic()
        {
            var testCode = @"public class Foo<T, U>
    where T : System.IDisposable {|GM0053:where U : System.ICloneable|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
