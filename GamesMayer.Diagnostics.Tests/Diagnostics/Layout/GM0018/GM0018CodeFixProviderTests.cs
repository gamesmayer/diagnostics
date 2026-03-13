namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0018CodeFixProviderTests
    {
        [Fact]
        public async Task CaseSwitchLabel_MultiLine_Fix()
        {
            var testCode = @"class Foo
{
    void M(int value)
    {
        switch (value)
        {
            {|GM0018:case
                1:|}
                break;
        }
    }
}";
            var fixedCode = @"class Foo
{
    void M(int value)
    {
        switch (value)
        {
            case 1:
                break;
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0018Analyzer, GM0018CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CasePatternSwitchLabel_MultiLine_Fix()
        {
            var testCode = @"class Foo
{
    void M(object value)
    {
        switch (value)
        {
            {|GM0018:case
                string s:|}
                break;
        }
    }
}";
            var fixedCode = @"class Foo
{
    void M(object value)
    {
        switch (value)
        {
            case string s:
                break;
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0018Analyzer, GM0018CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}