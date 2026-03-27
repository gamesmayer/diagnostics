namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0057CodeFixProviderTests
    {
        [Fact]
        public async Task MethodWithBlankLineBetweenDeclarationAndFirstConstraint_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
{|GM0057:
|}        where T : System.IDisposable { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable { }
}";
            var test = new CSharpCodeFixTest<GM0057Analyzer, GM0057CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodWithMultipleBlankLinesBetweenDeclarationAndFirstConstraint_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
{|GM0057:
|}{|GM0057:
|}        where T : System.IDisposable { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable { }
}";
            var test = new CSharpCodeFixTest<GM0057Analyzer, GM0057CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassWithBlankLineBetweenDeclarationAndFirstConstraint_Fix()
        {
            var testCode = @"public class Foo<T>
{|GM0057:
|}    where T : System.IDisposable
{
}";
            var fixedCode = @"public class Foo<T>
    where T : System.IDisposable
{
}";
            var test = new CSharpCodeFixTest<GM0057Analyzer, GM0057CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
