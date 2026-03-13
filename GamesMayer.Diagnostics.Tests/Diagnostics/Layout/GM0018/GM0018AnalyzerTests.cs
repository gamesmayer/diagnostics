namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0018Analyzer>;

    public class GM0018AnalyzerTests
    {
        [Fact]
        public async Task CaseSwitchLabel_SingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(int value)
    {
        switch (value)
        {
            case 1:
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CasePatternSwitchLabel_SingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(object value)
    {
        switch (value)
        {
            case string s:
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DefaultSwitchLabel_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(int value)
    {
        switch (value)
        {
            default:
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CaseSwitchLabel_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(int value)
    {
        switch (value)
        {
            {|GM0018:case
                1:|}
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CasePatternSwitchLabel_MultiLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(object value)
    {
        switch (value)
        {
            {|GM0018:case
                string s:|}
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CasePatternSwitchLabel_TypeAndIdentifierOnSeparateLines_Diagnostic()
        {
            var testCode = @"class Foo
{
    class SurfboardRewardEntity { }
    void M(object value)
    {
        switch (value)
        {
            {|GM0018:case
                SurfboardRewardEntity surfboardReward:|}
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}