namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0043CodeFixProviderTests
    {
        [Fact]
        public async Task ThreeOperands_Or_AllSameLine_Fix()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a || {|GM0043:b|} || {|GM0043:c|};
    }
}
";
            var fixedCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a ||
            b ||
            c;
    }
}
";
            var test = new CSharpCodeFixTest<GM0043Analyzer, GM0043CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ThreeOperands_And_AllSameLine_Fix()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a && {|GM0043:b|} && {|GM0043:c|};
    }
}
";
            var fixedCode = @"
class Test {
    public bool Method(bool a, bool b, bool c) {
        return a &&
            b &&
            c;
    }
}
";
            var test = new CSharpCodeFixTest<GM0043Analyzer, GM0043CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FourOperands_Or_AllSameLine_Fix()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b, bool c, bool d) {
        return a || {|GM0043:b|} || {|GM0043:c|} || {|GM0043:d|};
    }
}
";
            var fixedCode = @"
class Test {
    public bool Method(bool a, bool b, bool c, bool d) {
        return a ||
            b ||
            c ||
            d;
    }
}
";
            var test = new CSharpCodeFixTest<GM0043Analyzer, GM0043CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ThreeOperands_Or_AllOnOwnLines_NoFix()
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
            var test = new CSharpCodeFixTest<GM0043Analyzer, GM0043CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task TwoOperands_Or_NoFix()
        {
            var testCode = @"
class Test {
    public bool Method(bool a, bool b) {
        return a || b;
    }
}
";
            var test = new CSharpCodeFixTest<GM0043Analyzer, GM0043CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task TwoOperands_Or_WithIsExpressions_Fix()
        {
            var testCode = @"
class A { }
class B { }
class Test {
    public bool Method(object n) {
        return n is A || {|GM0043:n is B|};
    }
}
";
            var fixedCode = @"
class A { }
class B { }
class Test {
    public bool Method(object n) {
        return n is A ||
            n is B;
    }
}
";
            var test = new CSharpCodeFixTest<GM0043Analyzer, GM0043CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0043.threshold = 2"));
            await test.RunAsync();
        }
    }
}
