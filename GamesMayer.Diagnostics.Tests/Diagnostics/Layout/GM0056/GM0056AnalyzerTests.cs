namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0056Analyzer>;

    public class GM0056AnalyzerTests
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
        public async Task MethodWithSingleLineConstraint_NoDiagnostic()
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
        public async Task MethodWithConstraintSplitAfterColon_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
        {|GM0056:where T :
            System.IDisposable|} { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithConstraintSplitBetweenMultipleTypes_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
        {|GM0056:where T : System.IDisposable,
            System.ICloneable|} { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithSingleLineConstraint_NoDiagnostic()
        {
            var testCode = @"public class Foo<T>
    where T : System.IDisposable
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassWithConstraintSplitAcrossLines_Diagnostic()
        {
            var testCode = @"public class Foo<T>
    {|GM0056:where T :
        System.IDisposable|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodWithMultipleConstraintsOneMultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
        {|GM0056:where U : System.ICloneable,
            System.IComparable|} { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
