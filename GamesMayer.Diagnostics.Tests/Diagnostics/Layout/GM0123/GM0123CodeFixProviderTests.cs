namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0123CodeFixProviderTests
    {
        [Fact]
        public async Task FirstMember_OnOpenBraceLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        { {|GM0123:A = 1|},
            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            var fixedCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1,
            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            var test = new CSharpCodeFixTest<GM0123Analyzer, GM0123CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task AdjacentMembers_OnSameLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1, {|GM0123:B = 2|}
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            var fixedCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1,
            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            var test = new CSharpCodeFixTest<GM0123Analyzer, GM0123CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ObjectAsArgument_AdjacentMembers_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new Foo
        {
            A = 1, {|GM0123:B = 2|}
        });
    }

    void Use(Foo f) { }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            var fixedCode = @"class C
{
    void M()
    {
        Use(new Foo
        {
            A = 1,
            B = 2
        });
    }

    void Use(Foo f) { }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            var test = new CSharpCodeFixTest<GM0123Analyzer, GM0123CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
