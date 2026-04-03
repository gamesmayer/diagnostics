namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0115CodeFixProviderTests
    {
        [Fact]
        public async Task OpenBrace_OverIndented_Fix()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
        {|GM0115:{|}
        0,
        1
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
            var test = new CSharpCodeFixTest<GM0115Analyzer, GM0115CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CloseBrace_UnderIndented_Fix()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
{|GM0115:}|};
}";
            var fixedCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
    };
}";
            var test = new CSharpCodeFixTest<GM0115Analyzer, GM0115CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task LocalVariable_OpenBrace_OverIndented_Fix()
        {
            var testCode = @"class C
{
    void M()
    {
        int[] arr = new int[]
            {|GM0115:{|}
            0,
            1
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
            var test = new CSharpCodeFixTest<GM0115Analyzer, GM0115CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
