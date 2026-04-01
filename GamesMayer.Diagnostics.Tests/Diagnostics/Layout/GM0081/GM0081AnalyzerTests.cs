namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0081Analyzer>;

    public class GM0081AnalyzerTests
    {
        [Fact]
        public async Task CaseLabel_CorrectIndentation_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
                break;
            case 2:
                break;
            default:
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CaseLabel_SameColumnAsSwitch_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
        {|GM0081:case 1:|}
            break;
        {|GM0081:default:|}
            break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CaseLabel_OverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
                {|GM0081:case 1:|}
                    break;
                {|GM0081:default:|}
                    break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DefaultLabel_CorrectIndentation_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
                break;
            default:
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleCases_MixedIndentation_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
                break;
        {|GM0081:case 2:|}
                break;
            default:
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedSwitch_BothCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
                switch (2)
                {
                    case 2:
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedSwitch_InnerWrongIndentation_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        switch (1)
        {
            case 1:
                switch (2)
                {
                {|GM0081:case 2:|}
                    break;
                }
                break;
            default:
                break;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
