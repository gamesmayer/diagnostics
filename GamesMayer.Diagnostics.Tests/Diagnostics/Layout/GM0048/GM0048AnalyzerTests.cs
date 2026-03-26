namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0048Analyzer>;

    public class GM0048AnalyzerTests
    {
        [Fact]
        public async Task SimpleLambda_SingleLine_NoDiagnostic()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        Func<int, int> f = x => x + 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ParenthesizedLambda_SingleLine_NoDiagnostic()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        Func<int, int, int> f = (x, y) => x + y;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SimpleLambda_WithBraces_BodyOnNextLine_NoDiagnostic()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        Action<int> f = x =>
        {
            _ = x;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SimpleLambda_BodyOnNextLine_Diagnostic()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        Func<int, int> f = x =>
            {|GM0048:x + 1|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ParenthesizedLambda_BodyOnNextLine_Diagnostic()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        Func<int, int, int> f = (x, y) =>
            {|GM0048:x + y|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SimpleLambda_BodyOnSameLineButMultiLine_NoDiagnostic()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        Func<int, int> f = x => x +
            1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBodiedProperty_SingleLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    int[] items = new int[0];
    int[] Items => items;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBodiedProperty_BodyOnNextLine_Diagnostic()
        {
            var testCode = @"using System.Linq;
class C
{
    string[] characters = new string[0];
    string[] CharacterKeys =>
        {|GM0048:characters
            .Select(c => c)
            .ToArray()|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBodiedMethod_BodyOnNextLine_Diagnostic()
        {
            var testCode = @"class C
{
    int GetValue() =>
        {|GM0048:42|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
