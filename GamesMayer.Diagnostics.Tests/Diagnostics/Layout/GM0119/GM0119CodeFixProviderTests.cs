namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0119CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineBetweenItems_Fix()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0,
{|GM0119:
|}        1
    };
}";
            var fixedCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
    };
}";
            var test = new CSharpCodeFixTest<GM0119Analyzer, GM0119CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenItems_Fix()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0,
{|GM0119:
|}{|GM0119:
|}        1
    };
}";
            var fixedCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
    };
}";
            var test = new CSharpCodeFixTest<GM0119Analyzer, GM0119CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task LocalVariable_BlankLineBetweenItems_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        int[] arr = new int[]
        {
            0,
{|GM0119:
|}            1
        };
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
        int[] arr = new int[]
        {
            0,
            1
        };
    }
}";
            var test = new CSharpCodeFixTest<GM0119Analyzer, GM0119CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
