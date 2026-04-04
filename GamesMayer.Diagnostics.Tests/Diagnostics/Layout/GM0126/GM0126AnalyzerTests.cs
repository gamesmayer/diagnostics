namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0126AnalyzerTests
    {
        private static CSharpAnalyzerTest<GM0126Analyzer, XUnitVerifier> CreateTest(string testCode)
            => new CSharpAnalyzerTest<GM0126Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
                MarkupOptions = MarkupOptions.UseFirstDescriptor,
            };

        [Fact]
        public async Task ThreeParams_AllOnOneLine_NoDiagnostic()
        {
            var testCode = @"
class Test {
    public void Method(int a, int b, int c) { }
}
";
            await CreateTest(testCode).RunAsync();
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
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task FourParams_AllOnOneLine_ReportsAllParams()
        {
            var testCode = @"
class Test {
    public void Method({|GM0126:int a|}, {|GM0126:int b|}, {|GM0126:int c|}, {|GM0126:int d|}) { }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task FiveParams_AllOnOneLine_ReportsAllParams()
        {
            var testCode = @"
class Test {
    public void Method({|GM0126:int a|}, {|GM0126:int b|}, {|GM0126:int c|}, {|GM0126:int d|}, {|GM0126:int e|}) { }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task FourParams_FirstOnSameLineAsOpenParen_ReportsFirstParam()
        {
            var testCode = @"
class Test {
    public void Method({|GM0126:int a|},
        int b,
        int c,
        int d) { }
}
";
            await CreateTest(testCode).RunAsync();
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
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task TwoParams_MultiLine_OpenParenOnOwnLine_Diagnostic()
        {
            var testCode = @"
class Test {
    public void Method{|GM0126:(
        int a,
        int b)|}{ }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task OneParam_MultiLine_OpenParenOnOwnLine_Diagnostic()
        {
            var testCode = @"
class Test {
    public void Method{|GM0126:(
        int a)|}{ }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdTwo_TwoParams_Diagnostic()
        {
            var testCode = @"
class Test {
    public void Method({|GM0126:int a|}, {|GM0126:int b|}) { }
}
";
            var test = CreateTest(testCode);
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
            var test = CreateTest(testCode);
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
    public void Method({|GM0126:int a|}, {|GM0126:int b|}, {|GM0126:int c|}, {|GM0126:int d|}) { }
}
";
            var test = CreateTest(testCode);
            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0126.threshold = 4"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdFive_ThreeParams_MultiLine_Diagnostic()
        {
            var testCode = @"
class Test {
    public void Method{|GM0126:(
        int a,
        int b,
        int c)|}{ }
}
";
            var test = CreateTest(testCode);
            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0126.threshold = 5"));

            await test.RunAsync();
        }
    }
}
