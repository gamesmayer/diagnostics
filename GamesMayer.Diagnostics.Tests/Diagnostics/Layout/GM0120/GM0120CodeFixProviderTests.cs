namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0120CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineBetweenMembers_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1,
{|GM0120:
|}            B = 2
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
            var test = new CSharpCodeFixTest<GM0120Analyzer, GM0120CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenMembers_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1,
{|GM0120:
|}{|GM0120:
|}            B = 2
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
            var test = new CSharpCodeFixTest<GM0120Analyzer, GM0120CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ImplicitObjectCreation_BlankLineBetweenMembers_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo x = new()
        {
            A = 1,
{|GM0120:
|}            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            var fixedCode = @"class C
{
    void M()
    {
        Foo x = new()
        {
            A = 1,
            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            var test = new CSharpCodeFixTest<GM0120Analyzer, GM0120CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
