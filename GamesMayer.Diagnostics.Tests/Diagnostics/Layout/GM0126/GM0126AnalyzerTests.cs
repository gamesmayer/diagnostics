namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0126Analyzer>;

    public class GM0126AnalyzerTests
    {
        [Fact]
        public async Task ThreeParams_AllOnOneLine_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public void Method(int a, int b, int c) { }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FourParams_AllOnOwnLines_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public void Method(
        int a,
        int b,
        int c,
        int d) { }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FourParams_AllOnOneLine_Diagnostic()
        {
            var testCode = @"
class Test {
    public void Method(int a, {|GM0126:int b|}, {|GM0126:int c|}, {|GM0126:int d|}) { }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FiveParams_AllOnOneLine_Diagnostic()
        {
            var testCode = @"
class Test {
    public void Method(int a, {|GM0126:int b|}, {|GM0126:int c|}, {|GM0126:int d|}, {|GM0126:int e|}) { }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FourParams_MixedLines_Diagnostic()
        {
            var testCode = @"
class Test {
    public void Method(
        int a,
        int b, {|GM0126:int c|}, {|GM0126:int d|}) { }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConfiguredThresholdTwo_TwoParams_Diagnostic()
        {
            var testCode = @"
class Test {
    public void Method(int a, {|GM0126:int b|}) { }
}
";
            var test = new CSharpAnalyzerTest<GM0126Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0126.threshold = 2"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdFive_FourParams_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public void Method(int a, int b, int c, int d) { }
}
";
            var test = new CSharpAnalyzerTest<GM0126Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0126.threshold = 5"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdFour_FourParams_Diagnostic()
        {
            var testCode = @"
class Test {
    public void Method(int a, {|GM0126:int b|}, {|GM0126:int c|}, {|GM0126:int d|}) { }
}
";
            var test = new CSharpAnalyzerTest<GM0126Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0126.threshold = 4"));

            await test.RunAsync();
        }
    }
}
