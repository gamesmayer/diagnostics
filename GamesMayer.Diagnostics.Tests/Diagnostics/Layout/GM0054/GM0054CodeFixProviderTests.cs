namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0054CodeFixProviderTests
    {
        [Fact]
        public async Task MethodWithConstraintNotIndented_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
{|GM0054:where T : System.IDisposable|} { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable { }
}";
            var test = new CSharpCodeFixTest<GM0054Analyzer, GM0054CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodWithConstraintSameIndentAsDeclaration_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
    {|GM0054:where T : System.IDisposable|} { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable { }
}";
            var test = new CSharpCodeFixTest<GM0054Analyzer, GM0054CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodWithConstraintOverIndented_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
            {|GM0054:where T : System.IDisposable|} { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable { }
}";
            var test = new CSharpCodeFixTest<GM0054Analyzer, GM0054CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassWithConstraintWronglyIndented_Fix()
        {
            var testCode = @"public class Foo<T>
{|GM0054:where T : System.IDisposable|}
{
}";
            var fixedCode = @"public class Foo<T>
    where T : System.IDisposable
{
}";
            var test = new CSharpCodeFixTest<GM0054Analyzer, GM0054CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodWithMultipleConstraintsWronglyIndented_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
{|GM0054:where T : System.IDisposable|}
{|GM0054:where U : System.ICloneable|} { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
        where U : System.ICloneable { }
}";
            var test = new CSharpCodeFixTest<GM0054Analyzer, GM0054CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
