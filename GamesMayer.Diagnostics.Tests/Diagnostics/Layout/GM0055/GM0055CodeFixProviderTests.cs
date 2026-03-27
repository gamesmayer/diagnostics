namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0055CodeFixProviderTests
    {
        [Fact]
        public async Task MethodWithBlankLineBetweenConstraints_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
{|GM0055:
|}        where U : System.ICloneable { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
        where U : System.ICloneable { }
}";
            var test = new CSharpCodeFixTest<GM0055Analyzer, GM0055CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodWithMultipleBlankLinesBetweenConstraints_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
{|GM0055:
|}{|GM0055:
|}        where U : System.ICloneable { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
        where U : System.ICloneable { }
}";
            var test = new CSharpCodeFixTest<GM0055Analyzer, GM0055CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassWithBlankLineBetweenConstraints_Fix()
        {
            var testCode = @"public class Foo<T, U>
    where T : System.IDisposable
{|GM0055:
|}    where U : System.ICloneable
{
}";
            var fixedCode = @"public class Foo<T, U>
    where T : System.IDisposable
    where U : System.ICloneable
{
}";
            var test = new CSharpCodeFixTest<GM0055Analyzer, GM0055CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
