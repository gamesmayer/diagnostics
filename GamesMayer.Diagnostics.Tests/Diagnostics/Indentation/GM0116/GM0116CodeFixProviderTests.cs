namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0116CodeFixProviderTests
    {
        [Fact]
        public async Task Items_UnderIndented_Fix()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
    {|GM0116:0|},
    {|GM0116:1|}
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
            var test = new CSharpCodeFixTest<GM0116Analyzer, GM0116CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Items_OverIndented_Fix()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
            {|GM0116:0|},
            {|GM0116:1|}
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
            var test = new CSharpCodeFixTest<GM0116Analyzer, GM0116CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task LocalVariable_Items_UnderIndented_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        int[] arr = new int[]
        {
        {|GM0116:0|},
        {|GM0116:1|}
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
            var test = new CSharpCodeFixTest<GM0116Analyzer, GM0116CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
