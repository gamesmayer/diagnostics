namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0112CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineBetweenConsecutiveDeclarations_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        int a = 1;
{|GM0112:
|}        int b = 2;
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        int a = 1;
        int b = 2;
    }
}";
            var test = new CSharpCodeFixTest<GM0112Analyzer, GM0112CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenConsecutiveDeclarations_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        int a = 1;
{|GM0112:
|}{|GM0112:
|}        int b = 2;
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        int a = 1;
        int b = 2;
    }
}";
            var test = new CSharpCodeFixTest<GM0112Analyzer, GM0112CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}