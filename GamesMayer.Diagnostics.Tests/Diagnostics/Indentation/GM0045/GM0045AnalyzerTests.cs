namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0045Analyzer>;

    public class GM0045AnalyzerTests
    {
        [Fact]
        public async Task ExpressionBody_SameLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x => x + 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBody_OnNextLine_CorrectIndent_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
            x + 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlockBody_OnNextLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
        {
            return x + 1;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ParenthesizedLambda_OnNextLine_CorrectIndent_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int, int> f = (x, y) =>
            x + y;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBody_OnNextLine_SameIndentAsArrow_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
        {|GM0045:x + 1|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBody_OnNextLine_LessIndentThanArrow_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
    {|GM0045:x + 1|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBody_OnNextLine_OverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
                {|GM0045:x + 1|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ParenthesizedLambda_OnNextLine_WrongIndent_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int, int> f = (x, y) =>
        {|GM0045:x + y|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBody_IsType_OnNextLine_WrongIndent_Diagnostic()
        {
            var testCode = @"
class A { }
class C
{
    void M()
    {
        System.Func<object, bool> f = n =>
        {|GM0045:n is A|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBody_IsType_OnNextLine_CorrectIndent_NoDiagnostic()
        {
            var testCode = @"
class A { }
class C
{
    void M()
    {
        System.Func<object, bool> f = n =>
            n is A;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBodiedProperty_OnNextLine_CorrectIndent_NoDiagnostic()
        {
            var testCode = @"using System.Linq;
class C
{
    string[] characters = new string[0];
    string[] CharacterKeys =>
        characters.Select(c => c).ToArray();
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBodiedProperty_OnNextLine_WrongIndent_Diagnostic()
        {
            var testCode = @"using System.Linq;
class C
{
    string[] characters = new string[0];
    string[] CharacterKeys =>
    {|GM0045:characters.Select(c => c).ToArray()|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBodiedMethod_OnNextLine_WrongIndent_Diagnostic()
        {
            var testCode = @"class C
{
    int GetValue() =>
    {|GM0045:42|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
