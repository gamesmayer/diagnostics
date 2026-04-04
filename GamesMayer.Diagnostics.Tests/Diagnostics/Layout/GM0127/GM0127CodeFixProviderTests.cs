namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0127CodeFixProviderTests
    {
        [Fact]
        public async Task FourArgs_AllOnOneLine_Fix()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c, int d) { }
    public void Method() {
        Call({|GM0127:1|}, {|GM0127:2|}, {|GM0127:3|}, {|GM0127:4|});
    }
}
";
            var fixedCode = @"
class Test {
    void Call(int a, int b, int c, int d) { }
    public void Method() {
        Call
        (
            1,
            2,
            3,
            4
        );
    }
}
";
            var test = new CSharpCodeFixTest<GM0127Analyzer, GM0127CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FiveArgs_AllOnOneLine_Fix()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c, int d, int e) { }
    public void Method() {
        Call({|GM0127:1|}, {|GM0127:2|}, {|GM0127:3|}, {|GM0127:4|}, {|GM0127:5|});
    }
}
";
            var fixedCode = @"
class Test {
    void Call(int a, int b, int c, int d, int e) { }
    public void Method() {
        Call
        (
            1,
            2,
            3,
            4,
            5
        );
    }
}
";
            var test = new CSharpCodeFixTest<GM0127Analyzer, GM0127CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FourArgs_FirstOnSameLineAsOpenParen_Fix()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c, int d) { }
    public void Method() {
        Call({|GM0127:1|},
            2,
            3,
            4);
    }
}
";
            var fixedCode = @"
class Test {
    void Call(int a, int b, int c, int d) { }
    public void Method() {
        Call
        (
            1,
            2,
            3,
            4
        );
    }
}
";
            var test = new CSharpCodeFixTest<GM0127Analyzer, GM0127CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FourArgs_ObjectCreation_Fix()
        {
            var testCode = @"
class Foo {
    public Foo(int a, int b, int c, int d) { }
}
class Test {
    public void Method() {
        var x = new Foo({|GM0127:1|}, {|GM0127:2|}, {|GM0127:3|}, {|GM0127:4|});
    }
}
";
            var fixedCode = @"
class Foo {
    public Foo(int a, int b, int c, int d) { }
}
class Test {
    public void Method() {
        var x = new Foo
        (
            1,
            2,
            3,
            4
        );
    }
}
";
            var test = new CSharpCodeFixTest<GM0127Analyzer, GM0127CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ThreeArgs_NoFix()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c) { }
    public void Method() {
        Call(1, 2, 3);
    }
}
";
            var test = new CSharpCodeFixTest<GM0127Analyzer, GM0127CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }
    }
}
