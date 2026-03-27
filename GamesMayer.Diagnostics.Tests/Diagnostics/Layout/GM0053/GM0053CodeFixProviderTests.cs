namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0053CodeFixProviderTests
    {
        [Fact]
        public async Task MethodWithConstraintOnSameLineAsDeclaration_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T>() {|GM0053:where T : System.IDisposable|} { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable { }
}";
            var test = new CSharpCodeFixTest<GM0053Analyzer, GM0053CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassWithConstraintOnSameLineAsDeclaration_Fix()
        {
            var testCode = @"public class Foo<T> {|GM0053:where T : System.IDisposable|}
{
}";
            var fixedCode = @"public class Foo<T>
    where T : System.IDisposable
{
}";
            var test = new CSharpCodeFixTest<GM0053Analyzer, GM0053CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodWithSecondConstraintOnSameLineAsFirst_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable {|GM0053:where U : System.ICloneable|} { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
        where U : System.ICloneable { }
}";
            var test = new CSharpCodeFixTest<GM0053Analyzer, GM0053CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodWithAllConstraintsOnSameLine_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>() {|GM0053:where T : System.IDisposable|} {|GM0053:where U : System.ICloneable|} { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
        where U : System.ICloneable { }
}";
            var test = new CSharpCodeFixTest<GM0053Analyzer, GM0053CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
