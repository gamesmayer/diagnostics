namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0135CodeFixProviderTests
    {
        [Fact]
        public async Task UnnamedArgumentWithOtherNamed_Fix()
        {
            var testCode = @"
class Foo
{
    void Bar(int a, int b, int c) { }

    void Test()
    {
        Bar({|GM0135:1|}, b: 2, {|GM0135:3|});
    }
}
";
            var fixedCode = @"
class Foo
{
    void Bar(int a, int b, int c) { }

    void Test()
    {
        Bar(a: 1, b: 2, c: 3);
    }
}
";

            var test = new CSharpCodeFixTest<GM0135Analyzer, GM0135CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task ConstructorCall_UnnamedArgument_Fix()
        {
            var testCode = @"
class Foo
{
    public Foo(int x, int y) { }

    static Foo Create()
    {
        return new Foo({|GM0135:1|}, y: 2);
    }
}
";
            var fixedCode = @"
class Foo
{
    public Foo(int x, int y) { }

    static Foo Create()
    {
        return new Foo(x: 1, y: 2);
    }
}
";

            var test = new CSharpCodeFixTest<GM0135Analyzer, GM0135CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
