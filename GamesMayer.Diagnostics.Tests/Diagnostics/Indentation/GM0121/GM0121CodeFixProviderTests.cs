namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0121CodeFixProviderTests
    {
        [Fact]
        public async Task OpenBrace_OverIndented_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
            {|GM0121:{|}
            A = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            var fixedCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            var test = new CSharpCodeFixTest<GM0121Analyzer, GM0121CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CloseBrace_UnderIndented_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1
{|GM0121:}|};
    }
}

class Foo { public int A { get; set; } }";
            var fixedCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            var test = new CSharpCodeFixTest<GM0121Analyzer, GM0121CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ImplicitObjectCreation_OpenBrace_OverIndented_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo x = new()
            {|GM0121:{|}
            A = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            var fixedCode = @"class C
{
    void M()
    {
        Foo x = new()
        {
            A = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            var test = new CSharpCodeFixTest<GM0121Analyzer, GM0121CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FieldDeclaration_OpenBrace_OverIndented_Fix()
        {
            var testCode = @"class C
{
    Foo f = new Foo
        {|GM0121:{|}
        A = 1
    };
}

class Foo { public int A { get; set; } }";
            var fixedCode = @"class C
{
    Foo f = new Foo
    {
        A = 1
    };
}

class Foo { public int A { get; set; } }";
            var test = new CSharpCodeFixTest<GM0121Analyzer, GM0121CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
