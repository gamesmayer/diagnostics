namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0058CodeFixProviderTests
    {
        [Fact]
        public async Task ClassWithColonOnNextLine_Fix()
        {
            var testCode = @"class Foo
    {|GM0058:: System.IDisposable|}
{
    public void Dispose() { }
}";
            var fixedCode = @"class Foo : System.IDisposable
{
    public void Dispose() { }
}";
            var test = new CSharpCodeFixTest<GM0058Analyzer, GM0058CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassWithColonAndBaseTypeOnSeparateLines_Fix()
        {
            var testCode = @"class Foo
    {|GM0058::
    System.IDisposable|}
{
    public void Dispose() { }
}";
            var fixedCode = @"class Foo : System.IDisposable
{
    public void Dispose() { }
}";
            var test = new CSharpCodeFixTest<GM0058Analyzer, GM0058CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassWithBaseTypesOnDifferentLines_Fix()
        {
            var testCode = @"interface IBar { }
class Foo
    {|GM0058:: System.IDisposable,
    IBar|}
{
    public void Dispose() { }
}";
            var fixedCode = @"interface IBar { }
class Foo : System.IDisposable, IBar
{
    public void Dispose() { }
}";
            var test = new CSharpCodeFixTest<GM0058Analyzer, GM0058CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
