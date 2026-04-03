namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0115Analyzer>;

    public class GM0115AnalyzerTests
    {
        [Fact]
        public async Task ArrayInitializer_BracesCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitArrayInitializer_BracesCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new[]
    {
        0,
        1
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_SingleLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[] { 0, 1 };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_OpenBraceOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
        {|GM0115:{|}
        0,
        1
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_CloseBraceUnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
{|GM0115:}|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_BothBracesWrongIndent_TwoDiagnostics()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
        {|GM0115:{|}
        0,
        1
        {|GM0115:}|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LocalVariable_ArrayInitializer_BracesCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LocalVariable_ArrayInitializer_OpenBraceOverIndented_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NotAnInitialValue_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new int[]
        {
            0,
            1
        });
    }

    void Use(int[] arr) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
