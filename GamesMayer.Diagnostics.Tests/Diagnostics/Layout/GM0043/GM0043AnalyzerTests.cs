namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0043Analyzer>;

    public class GM0043AnalyzerTests
    {
        [Fact]
        public async Task TwoOperands_Or_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b) {
        return a || b;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TwoOperands_And_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b) {
        return a && b;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeOperands_Or_AllOnOwnLines_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a
            || b
            || c;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeOperands_And_AllOnOwnLines_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a
            && b
            && c;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeOperands_Or_AllSameLine_Diagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a || {|GM0043:b|} || {|GM0043:c|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeOperands_And_AllSameLine_Diagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a && {|GM0043:b|} && {|GM0043:c|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FourOperands_Or_AllSameLine_Diagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c, bool d) {
        return a || {|GM0043:b|} || {|GM0043:c|} || {|GM0043:d|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeOperands_Or_MixedLines_Diagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a
            || b || {|GM0043:c|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeOperands_Or_InIfCondition_AllSameLine_Diagnostic()
        {
            var testCode = @"
class Test {
    public void Method(bool a, bool b, bool c) {
        if (a || {|GM0043:b|} || {|GM0043:c|}) { }
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeOperands_Or_InVariableInitializer_AllSameLine_Diagnostic()
        {
            var testCode = @"
class Test {
    public void Method(bool a, bool b, bool c) {
        bool result = a || {|GM0043:b|} || {|GM0043:c|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeOperands_And_InReturnStatement_AllSameLine_Diagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a && {|GM0043:b|} && {|GM0043:c|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConfiguredThresholdTwo_TwoOperands_Diagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b) {
        return a || {|GM0043:b|};
    }
}
";

            var test = new CSharpAnalyzerTest<GM0043Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0043.threshold = 2"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdFour_ThreeOperands_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a || b || c;
    }
}
";

            var test = new CSharpAnalyzerTest<GM0043Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0043.threshold = 4"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdFour_FourOperands_Diagnostic()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c, bool d) {
        return a || {|GM0043:b|} || {|GM0043:c|} || {|GM0043:d|};
    }
}
";

            var test = new CSharpAnalyzerTest<GM0043Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0043.threshold = 4"));

            await test.RunAsync();
        }

        [Fact]
        public async Task MixedOperators_InnerChainCheckedIndependently_NoDiagnostic()
        {
            // a && b || c has 2 operands for ||, and 2 operands for && — both below threshold of 3
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a && b || c;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
