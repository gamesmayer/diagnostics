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

        // ── Switch expression tests ───────────────────────────────────────────

        [Fact]
        public async Task SwitchExpression_Arms_CorrectIndentation_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M(int x)
    {
        return x switch
        {
            1 => 10,
            2 => 20,
            _ => 0
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchExpression_Arms_SameColumnAsStatement_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(int x)
    {
        return x switch
        {
        {|GM0081:1 => 10|},
        {|GM0081:2 => 20|},
        {|GM0081:_ => 0|}
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchExpression_Arms_OverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(int x)
    {
        return x switch
        {
                {|GM0081:1 => 10|},
                {|GM0081:2 => 20|},
                {|GM0081:_ => 0|}
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchExpression_SingleLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M(int x) => x switch { 1 => 10, _ => 0 };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchExpression_PatternArms_CorrectIndentation_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    string M(object o)
    {
        return o switch
        {
            int n => n.ToString(),
            string s => s,
            _ => string.Empty
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SwitchExpression_PatternArms_SameColumnAsStatement_Diagnostic()
        {
            var testCode = @"class Foo
{
    string M(object o)
    {
        return o switch
        {
        {|GM0081:int n => n.ToString()|},
        {|GM0081:string s => s|},
        {|GM0081:_ => string.Empty|}
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
