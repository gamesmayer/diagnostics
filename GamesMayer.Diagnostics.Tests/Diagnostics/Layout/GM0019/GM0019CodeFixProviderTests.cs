namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0019CodeFixProviderTests
    {
        [Fact]
        public async Task EmptyMethodBody_BracesOnDifferentLine_Fix()
        {
            var testCode = @"class Foo
{
    protected void Method(
        int a,
        int b,
        int c
    )
    {|GM0019:{
    }|}
}";
            var fixedCode = @"class Foo
{
    protected void Method(
        int a,
        int b,
        int c
    )
    { }
}";
            var test = new CSharpCodeFixTest<GM0019Analyzer, GM0019CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyClassBody_BracesOnDifferentLine_Fix()
        {
            var testCode = @"class Foo<
    T>
    where T : class
{|GM0019:{
}|}
";
            var fixedCode = @"class Foo<
    T>
    where T : class
{ }
";
            var test = new CSharpCodeFixTest<GM0019Analyzer, GM0019CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyIfBlock_BracesOnDifferentLine_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {|GM0019:{
        }|}
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        if (true)
        { }
    }
}";
            var test = new CSharpCodeFixTest<GM0019Analyzer, GM0019CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyConstructorBody_BracesWithoutSpace_Fix()
        {
            var testCode = @"class Foo
{
    public Foo() {|GM0019:{}|}
}";
            var fixedCode = @"class Foo
{
    public Foo() { }
}";
            var test = new CSharpCodeFixTest<GM0019Analyzer, GM0019CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
