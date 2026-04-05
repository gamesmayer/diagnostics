namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0121CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineBetweenDeclarationAndBaseInitializer_Fix()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x)
{|GM0121:|}
        : base(x)
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
    public Foo(int x)
        : base(x)
    {
    }
}
";

            var test = new CSharpCodeFixTest<GM0121Analyzer, GM0121CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task BlankLineBetweenDeclarationAndThisInitializer_Fix()
        {
            var testCode = @"
class Foo
{
    public Foo(int x) { }

    public Foo(int x, int y)
{|GM0121:|}
        : this(x)
    {
    }
}
";
            var fixedCode = @"
class Foo
{
    public Foo(int x) { }

    public Foo(int x, int y)
        : this(x)
    {
    }
}
";

            var test = new CSharpCodeFixTest<GM0121Analyzer, GM0121CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLines_Fix()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x)
{|GM0121:|}

        : base(x)
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
    public Foo(int x)
        : base(x)
    {
    }
}
";

            var test = new CSharpCodeFixTest<GM0121Analyzer, GM0121CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task BlankLineAfterColon_Fix()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x) :
{|GM0121:|}
        base(x)
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

            var test = new CSharpCodeFixTest<GM0121Analyzer, GM0121CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
