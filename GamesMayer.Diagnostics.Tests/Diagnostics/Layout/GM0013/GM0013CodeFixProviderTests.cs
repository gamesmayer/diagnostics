namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0013CodeFixProviderTests
    {
        [Fact]
        public async Task SwitchCase_SingleStatement_Fix()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
                {|GM0013:break;|}
        }
    }
}";
            var fixedCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
            {
                break;
            }
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0013Analyzer, GM0013CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task SwitchCase_MultipleStatements_Fix()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
                {|GM0013:DoSomething();
                break;|}
        }
    }

    void DoSomething() { }
}";
            var fixedCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            case 1:
            {
                DoSomething();
                break;
            }
        }
    }

    void DoSomething() { }
}";
            var test = new CSharpCodeFixTest<GM0013Analyzer, GM0013CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task DefaultCase_Fix()
        {
            var testCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            default:
                {|GM0013:break;|}
        }
    }
}";
            var fixedCode = @"class Foo
{
    void Method(int x)
    {
        switch (x)
        {
            default:
            {
                break;
            }
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0013Analyzer, GM0013CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
