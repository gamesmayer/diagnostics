namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0143Analyzer>;

    public class GM0143AnalyzerTests
    {
        // ── catch clause ──────────────────────────────────────────────────────

        [Fact]
        public async Task CatchOnOwnLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        }
        catch (System.Exception)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CatchOnSameLineAsClosingBrace_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        } {|GM0143:catch|} (System.Exception)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleCatches_SecondOnSameLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        }
        catch (System.InvalidOperationException)
        {
        } {|GM0143:catch|} (System.Exception)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── finally clause ────────────────────────────────────────────────────

        [Fact]
        public async Task FinallyOnOwnLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        }
        finally
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FinallyOnSameLineAsClosingBrace_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        } {|GM0143:finally|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FinallyAfterCatch_OnSameLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        }
        catch (System.Exception)
        {
        } {|GM0143:finally|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── else clause ───────────────────────────────────────────────────────

        [Fact]
        public async Task ElseOnOwnLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        }
        else
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseOnSameLineAsClosingBrace_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        } {|GM0143:else|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseIfOnSameLineAsClosingBrace_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        } {|GM0143:else|} if (false)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseIfChain_AllCorrect_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
        {
        }
        else if (false)
        {
        }
        else
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── single-line blocks ────────────────────────────────────────────────

        [Fact]
        public async Task SingleLineTryCatch_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        try { } catch (System.Exception) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleLineIfElse_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true) { } else { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── else without braces (no diagnostic) ──────────────────────────────

        [Fact]
        public async Task ElseWithoutBracesOnIfBody_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        if (true)
            return;
        else
            return;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── combined ──────────────────────────────────────────────────────────

        [Fact]
        public async Task TryCatchFinally_CatchAndFinallyOnSameLine_BothDiagnostics()
        {
            var testCode = @"class C
{
    void M()
    {
        try
        {
        } {|GM0143:catch|} (System.Exception)
        {
        } {|GM0143:finally|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
