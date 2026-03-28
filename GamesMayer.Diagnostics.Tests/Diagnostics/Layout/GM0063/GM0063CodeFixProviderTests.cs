namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0063CodeFixProviderTests
    {
        [Fact]
        public async Task Method_SingleLineBody_Fix()
        {
            var testCode = @"class Foo
{
    void M()
    {|GM0063:{ System.Console.WriteLine(); }|}
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        System.Console.WriteLine();
    }
}";
            var test = new CSharpCodeFixTest<GM0063Analyzer, GM0063CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Constructor_SingleLineBody_Fix()
        {
            var testCode = @"class Foo
{
    int _x;

    public Foo()
    {|GM0063:{ _x = 1; }|}
}";
            var fixedCode = @"class Foo
{
    int _x;

    public Foo()
    {
        _x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0063Analyzer, GM0063CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ParenthesizedLambda_SingleLineBlock_Fix()
        {
            var testCode = @"using System;

class Foo
{
    void M()
    {
        Action a = () => {|GM0063:{ System.Console.WriteLine(); }|};
    }
}";
            var fixedCode = @"using System;

class Foo
{
    void M()
    {
        Action a = () => {
            System.Console.WriteLine();
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0063Analyzer, GM0063CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
