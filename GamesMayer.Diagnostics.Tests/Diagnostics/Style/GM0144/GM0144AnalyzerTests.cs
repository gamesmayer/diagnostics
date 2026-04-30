namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0144Analyzer>;

    public class GM0144AnalyzerTests
    {
        [Fact]
        public async Task NotEqualsNull_PropertyAccess_Diagnostic()
        {
            var testCode = @"class C
{
    string M(string x)
    {
        return {|GM0144:x != null ? x.ToString() : null|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EqualsNull_PropertyAccess_Diagnostic()
        {
            var testCode = @"class C
{
    string M(string x)
    {
        return {|GM0144:x == null ? null : x.ToString()|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NotEqualsNull_FieldAccess_Diagnostic()
        {
            var testCode = @"class Foo { public string Value = """"; }
class C
{
    string M(Foo x)
    {
        return {|GM0144:x != null ? x.Value : null|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NotEqualsNull_MethodCallWithArgs_Diagnostic()
        {
            var testCode = @"class Foo { public string Get(int n) => """"; }
class C
{
    string M(Foo x)
    {
        return {|GM0144:x != null ? x.Get(1) : null|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NullConditionalAlreadyUsed_NoDiagnostic()
        {
            var testCode = @"class C
{
    string M(string x)
    {
        return x?.ToString();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NotEqualsNull_NonNullElse_NoDiagnostic()
        {
            var testCode = @"class C
{
    string M(string x)
    {
        return x != null ? x.ToString() : ""default"";
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NotEqualsNull_DifferentExpressionInBranch_NoDiagnostic()
        {
            var testCode = @"class C
{
    string M(string x, string y)
    {
        return x != null ? y.ToString() : null;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NotEqualsNull_NonMemberAccessBranch_NoDiagnostic()
        {
            var testCode = @"class C
{
    string M(string x)
    {
        return x != null ? x : null;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
