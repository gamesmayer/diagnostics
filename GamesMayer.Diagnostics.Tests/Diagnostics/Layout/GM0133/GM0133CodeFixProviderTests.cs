namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0133CodeFixProviderTests
    {
        [Fact]
        public async Task ConstructorWithBaseInitializerOnDeclarationLine_Fix()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x) : {|GM0133:base|}(x)
    {
    }
}
";
            var fixedCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x) :
        base(x)
    {
    }
}
";

            var test = new CSharpCodeFixTest<GM0133Analyzer, GM0133CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ConstructorWithThisInitializerOnDeclarationLine_Fix()
        {
            var testCode = @"
class Foo
{
    public Foo(int x) { }

    public Foo(int x, int y) : {|GM0133:this|}(x)
    {
    }
}
";
            var fixedCode = @"
class Foo
{
    public Foo(int x) { }

    public Foo(int x, int y) :
        this(x)
    {
    }
}
";

            var test = new CSharpCodeFixTest<GM0133Analyzer, GM0133CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
