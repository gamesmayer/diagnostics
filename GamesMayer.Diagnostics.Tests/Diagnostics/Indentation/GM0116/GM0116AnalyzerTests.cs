namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0116Analyzer>;

    public class GM0116AnalyzerTests
    {
        [Fact]
        public async Task ArrayInitializer_ItemsCorrectlyIndented_NoDiagnostic()
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
        public async Task ImplicitArrayInitializer_ItemsCorrectlyIndented_NoDiagnostic()
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
        public async Task ArrayInitializer_ItemOnSameLineAsOpenBrace_NoDiagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    { 0,
        1
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_ItemsUnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
    {|GM0116:0|},
    {|GM0116:1|}
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_ItemsOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
            {|GM0116:0|},
            {|GM0116:1|}
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LocalVariable_ArrayInitializer_ItemsCorrectlyIndented_NoDiagnostic()
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
        public async Task LocalVariable_ArrayInitializer_ItemsUnderIndented_Diagnostic()
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
