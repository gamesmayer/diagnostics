namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0024Analyzer>;

    public class GM0024AnalyzerTests
    {
        [Fact]
        public async Task CorrectlyIndentedLambda_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
            return;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InlineLambda_BraceOnSameLineAsArrow_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () => { return; };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CorrectlyIndentedAnonymousMethod_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = delegate
        {
            return;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InlineAnonymousMethod_BraceOnSameLineAsDelegate_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = delegate { return; };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CorrectlyIndentedSimpleLambda_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action<int> action = x =>
        {
            _ = x;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MisindentedOpeningBrace_Lambda_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
            {|GM0024:{|}
            return;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MisindentedStatement_Lambda_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
        {|GM0024:return|};
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MisindentedClosingBrace_Lambda_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
            return;
            {|GM0024:}|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MisindentedOpeningBrace_AnonymousMethod_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = delegate
            {|GM0024:{|}
            return;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MisindentedOpeningBrace_SimpleLambda_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action<int> action = x =>
            {|GM0024:{|}
            _ = x;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleViolations_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
        {|GM0024:return|};
            {|GM0024:}|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CorrectlyIndentedEmptyLambdaBody_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Action action = () =>
        {
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
