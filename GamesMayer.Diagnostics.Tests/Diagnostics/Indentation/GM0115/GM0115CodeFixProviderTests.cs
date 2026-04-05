namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0115CodeFixProviderTests
    {
        [Fact]
        public async Task ConstructorWithBaseInitializerNotIndented_Fix()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x) :
{|GM0115:base|}(x)
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

            var test = new CSharpCodeFixTest<GM0115Analyzer, GM0115CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ConstructorWithBaseInitializerOverIndented_Fix()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x) :
            {|GM0115:base|}(x)
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

            var test = new CSharpCodeFixTest<GM0115Analyzer, GM0115CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NestedClassConstructorWithBaseInitializerWronglyIndented_Fix()
        {
            var testCode = @"
class Outer
{
    class Bar
    {
        public Bar(int x) { }
    }

    class Foo : Bar
    {
        public Foo(int x) :
        {|GM0115:base|}(x)
        {
        }
    }
}
";
            var fixedCode = @"
class Outer
{
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
}
";

            var test = new CSharpCodeFixTest<GM0115Analyzer, GM0115CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
