namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0117Analyzer>;

    public class GM0117AnalyzerTests
    {
        [Fact]
        public async Task ArrayInitializer_EachItemOnOwnLine_NoDiagnostic()
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
        public async Task ImplicitArrayInitializer_EachItemOnOwnLine_NoDiagnostic()
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
        public async Task ArrayInitializer_FirstItemOnOpenBraceLine_Diagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    { {|GM0117:0|},
        1
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_AdjacentItemsOnSameLine_Diagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0, {|GM0117:1|}
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_MultipleItemsOnSameLine_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0, {|GM0117:1|}, {|GM0117:2|}
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LocalVariable_ArrayInitializer_EachItemOnOwnLine_NoDiagnostic()
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
        public async Task LocalVariable_ArrayInitializer_AdjacentItemsOnSameLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int[] arr = new int[]
        {
            0, {|GM0117:1|}
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
            0, 1
        });
    }

    void Use(int[] arr) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
