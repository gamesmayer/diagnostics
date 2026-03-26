namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0046Analyzer>;

    public class GM0046AnalyzerTests
    {
        [Fact]
        public async Task SingleLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null && a.Length > 0;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OperatorAtEndOfLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null &&
                 a.Length > 0;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeOperands_AllOperatorsAtEndOfLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(string a, string b)
    {
        bool result = a != null &&
            a.Length > 0 &&
            b != null;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArithmeticOperators_AtEndOfLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int x = 0 +
            1 -
            2 *
            3;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AndOperator_AtStartOfNextLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null
            {|GM0046:&&|} a.Length > 0;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArithmeticOperator_AtStartOfNextLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int x = 0 +
            1 -
            2
            {|GM0046:*|} 3;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeOperands_TwoViolations_Diagnostics()
        {
            var testCode = @"class C
{
    void M(string a, string b)
    {
        bool result = a != null
            {|GM0046:&&|} a.Length > 0
            {|GM0046:&&|} b != null;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeOperands_FirstOperatorViolation_OneDiagnostic()
        {
            var testCode = @"class C
{
    void M(string a, string b)
    {
        bool result = a != null
            {|GM0046:&&|} a.Length > 0 &&
            b != null;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OperatorOnOwnLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null
            {|GM0046:&&|}
            a.Length > 0;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MixedArithmeticAndLogical_Violation_Diagnostic()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null
            {|GM0046:&&|} a.Length +
            1 > 0;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
