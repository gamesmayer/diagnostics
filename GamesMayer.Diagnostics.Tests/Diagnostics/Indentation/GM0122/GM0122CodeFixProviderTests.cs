namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0122CodeFixProviderTests
    {
        [Fact]
        public async Task Members_UnderIndented_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
        {|GM0122:A|} = 1,
        {|GM0122:B|} = 2
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
            var test = new CSharpCodeFixTest<GM0122Analyzer, GM0122CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Members_OverIndented_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
                {|GM0122:A|} = 1,
                {|GM0122:B|} = 2
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
            var test = new CSharpCodeFixTest<GM0122Analyzer, GM0122CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FieldDeclaration_Members_OverIndented_Fix()
        {
            var testCode = @"class C
{
    Foo f = new Foo
    {
            {|GM0122:A|} = 1,
            {|GM0122:B|} = 2
    };
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            var fixedCode = @"class C
{
    Foo f = new Foo
    {
        A = 1,
        B = 2
    };
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            var test = new CSharpCodeFixTest<GM0122Analyzer, GM0122CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
