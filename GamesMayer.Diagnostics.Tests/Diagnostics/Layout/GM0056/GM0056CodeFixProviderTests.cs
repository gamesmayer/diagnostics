namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0056CodeFixProviderTests
    {
        [Fact]
        public async Task ConstraintSplitAfterColon_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
        {|GM0056:where T :
            System.IDisposable|} { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable { }
}";
            var test = new CSharpCodeFixTest<GM0056Analyzer, GM0056CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ConstraintSplitBetweenMultipleTypes_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T>()
        {|GM0056:where T : System.IDisposable,
            System.ICloneable|} { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T>()
        where T : System.IDisposable, System.ICloneable { }
}";
            var test = new CSharpCodeFixTest<GM0056Analyzer, GM0056CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassConstraintSplitAcrossLines_Fix()
        {
            var testCode = @"public class Foo<T>
    {|GM0056:where T :
        System.IDisposable|}
{
}";
            var fixedCode = @"public class Foo<T>
    where T : System.IDisposable
{
}";
            var test = new CSharpCodeFixTest<GM0056Analyzer, GM0056CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleConstraintsOneMultiLine_Fix()
        {
            var testCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
        {|GM0056:where U : System.ICloneable,
            System.IComparable|} { }
}";
            var fixedCode = @"class Foo
{
    public void Method<T, U>()
        where T : System.IDisposable
        where U : System.ICloneable, System.IComparable { }
}";
            var test = new CSharpCodeFixTest<GM0056Analyzer, GM0056CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
