namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0118CodeFixProviderTests
    {
        [Fact]
        public async Task MultiLineArgumentList_CommaOnOwnLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
        (
            1
            {|GM0118:,|}
            2
            {|GM0118:,|}
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            var fixedCode = @"class C
{
    void M()
    {
        Foo
        (
            1,
            2,
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            var test = new CSharpCodeFixTest<GM0118Analyzer, GM0118CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultiLineArgumentList_CommaAtStartOfNextItemLine_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
        (
            1
            {|GM0118:,|} 2,
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            var fixedCode = @"class C
{
    void M()
    {
        Foo
        (
            1, 2,
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            var test = new CSharpCodeFixTest<GM0118Analyzer, GM0118CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultiLineParameterList_CommaOnOwnLine_Fix()
        {
            var testCode = @"class C
{
    void Foo
    (
        int a
        {|GM0118:,|}
        int b
    )
    {
    }
}";
            var fixedCode = @"class C
{
    void Foo
    (
        int a,
        int b
    )
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0118Analyzer, GM0118CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
