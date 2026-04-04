namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0126CodeFixProviderTests
    {
        [Fact]
        public async Task FourParams_AllOnOneLine_Fix()
        {
            var testCode = @"
class Test {
    public void Method(int a, {|GM0126:int b|}, {|GM0126:int c|}, {|GM0126:int d|}) { }
}
";
            var fixedCode = @"
class Test {
    public void Method(
        int a,
        int b,
        int c,
        int d) { }
}
";
            var test = new CSharpCodeFixTest<GM0126Analyzer, GM0126CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FiveParams_AllOnOneLine_Fix()
        {
            var testCode = @"
class Test {
    public void Method(int a, {|GM0126:int b|}, {|GM0126:int c|}, {|GM0126:int d|}, {|GM0126:int e|}) { }
}
";
            var fixedCode = @"
class Test {
    public void Method(
        int a,
        int b,
        int c,
        int d,
        int e) { }
}
";
            var test = new CSharpCodeFixTest<GM0126Analyzer, GM0126CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ThreeParams_AllOnOwnLines_NoFix()
        {
            var testCode = @"
class Test {
    public void Method(int a, int b, int c) { }
}
";
            var test = new CSharpCodeFixTest<GM0126Analyzer, GM0126CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }
    }
}
