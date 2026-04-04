namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0126CodeFixProviderTests
    {
        private static CSharpCodeFixTest<GM0126Analyzer, GM0126CodeFixProvider, XUnitVerifier> CreateTest(
            string testCode,
            string fixedCode,
            int numberOfFixAllIterations = 1)
            => new CSharpCodeFixTest<GM0126Analyzer, GM0126CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                MarkupOptions = MarkupOptions.UseFirstDescriptor,
                NumberOfFixAllIterations = numberOfFixAllIterations,
            };

        [Fact]
        public async Task FourParams_AllOnOneLine_Fix()
        {
            var testCode = @"
class Test {
    public void Method({|GM0126:int a|}, {|GM0126:int b|}, {|GM0126:int c|}, {|GM0126:int d|}) { }
}
";
            var fixedCode = @"
class Test {
    public void Method
    (
        int a,
        int b,
        int c,
        int d
    ) { }
}
";
            await CreateTest(testCode, fixedCode).RunAsync();
        }

        [Fact]
        public async Task FiveParams_AllOnOneLine_Fix()
        {
            var testCode = @"
class Test {
    public void Method({|GM0126:int a|}, {|GM0126:int b|}, {|GM0126:int c|}, {|GM0126:int d|}, {|GM0126:int e|}) { }
}
";
            var fixedCode = @"
class Test {
    public void Method
    (
        int a,
        int b,
        int c,
        int d,
        int e
    ) { }
}
";
            await CreateTest(testCode, fixedCode).RunAsync();
        }

        [Fact]
        public async Task FourParams_FirstOnSameLineAsOpenParen_Fix()
        {
            var testCode = @"
class Test {
    public void Method({|GM0126:int a|},
        int b,
        int c,
        int d) { }
}
";
            var fixedCode = @"
class Test {
    public void Method
    (
        int a,
        int b,
        int c,
        int d
    ) { }
}
";
            await CreateTest(testCode, fixedCode).RunAsync();
        }

        [Fact]
        public async Task ThreeParams_AllOnOwnLines_NoFix()
        {
            var testCode = @"
class Test {
    public void Method(int a, int b, int c) { }
}
";
            await CreateTest(testCode, testCode, numberOfFixAllIterations: 0).RunAsync();
        }

        [Fact]
        public async Task TwoParams_MultiLine_OpenParenOnOwnLine_Fix()
        {
            var testCode = @"
class Test {
    public void Method{|GM0126:(
        int a,
        int b)|}{ }
}
";
            var fixedCode = @"
class Test {
    public void Method(int a, int b){ }
}
";
            await CreateTest(testCode, fixedCode).RunAsync();
        }

        [Fact]
        public async Task OneParam_MultiLine_OpenParenOnOwnLine_Fix()
        {
            var testCode = @"
class Test {
    public void Method{|GM0126:(
        int a)|}{ }
}
";
            var fixedCode = @"
class Test {
    public void Method(int a){ }
}
";
            await CreateTest(testCode, fixedCode).RunAsync();
        }
    }
}
