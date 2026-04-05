namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0119Analyzer>;

    public class GM0119AnalyzerTests
    {
        [Fact]
        public async Task ArrayInitializer_NoBlankLines_NoDiagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1,
        2
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitArrayInitializer_NoBlankLines_NoDiagnostic()
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
        public async Task ArrayInitializer_BlankLineBetweenItems_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_MultipleBlankLinesBetweenItems_MultipleDiagnostics()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_BlankLineBetweenMultiplePairs_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0,
{|GM0119:
|}        1,
{|GM0119:
|}        2
    };
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
        Use(new int[] { 0, 1 });
    }

    void Use(int[] arr) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Argument_ArrayInitializer_BlankLineBetweenItems_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new int[]
        {
            0,
{|GM0119:
|}            1
        });
    }

    void Use(int[] arr) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LocalVariable_ArrayInitializer_BlankLineBetweenItems_Diagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
