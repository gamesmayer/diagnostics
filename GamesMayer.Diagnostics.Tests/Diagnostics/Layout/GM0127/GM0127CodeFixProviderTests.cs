namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0127CodeFixProviderTests
    {
        private static CSharpCodeFixTest<GM0127Analyzer, GM0127CodeFixProvider, XUnitVerifier> CreateTest(
            string testCode,
            string fixedCode,
            int numberOfFixAllIterations = 1)
            => new CSharpCodeFixTest<GM0127Analyzer, GM0127CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                MarkupOptions = MarkupOptions.UseFirstDescriptor,
                NumberOfFixAllIterations = numberOfFixAllIterations,
            };

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
            await CreateTest(testCode, fixedCode).RunAsync();
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
            await CreateTest(testCode, fixedCode).RunAsync();
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
            await CreateTest(testCode, fixedCode).RunAsync();
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
            await CreateTest(testCode, fixedCode).RunAsync();
        }

        [Fact]
        public async Task OneArg_Lambda_Fix()
        {
            var testCode = @"
using System;

class Test {
    void Call(Action<int> action) { }
    public void Method() {
        Call({|GM0127:x => Console.WriteLine(x)|});
    }
}
";
            var fixedCode = @"
using System;

class Test {
    void Call(Action<int> action) { }
    public void Method() {
        Call
        (
            x => Console.WriteLine(x)
        );
    }
}
";
            await CreateTest(testCode, fixedCode).RunAsync();
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
            await CreateTest(testCode, testCode, numberOfFixAllIterations: 0).RunAsync();
        }

        [Fact]
        public async Task TwoArgs_MultiLine_OpenParenOnSameLine_Collapse()
        {
            var testCode = @"
class Test {
    void Call(int a, int b) { }
    public void Method() {
        Call{|GM0127:(1,
            2)|};
    }
}
";
            var fixedCode = @"
class Test {
    void Call(int a, int b) { }
    public void Method() {
        Call(1, 2);
    }
}
";
            await CreateTest(testCode, fixedCode).RunAsync();
        }

        [Fact]
        public async Task OneArg_MultiLine_OpenParenOnOwnLine_Collapse()
        {
            var testCode = @"
class Test {
    void Call(int a) { }
    public void Method() {
        Call{|GM0127:(
            1)|};
    }
}
";
            var fixedCode = @"
class Test {
    void Call(int a) { }
    public void Method() {
        Call(1);
    }
}
";
            await CreateTest(testCode, fixedCode).RunAsync();
        }

        [Fact]
        public async Task TwoArgs_MultiLine_OpenParenOnOwnLine_Collapse()
        {
            var testCode = @"
class Test {
    void Call(int a, int b) { }
    public void Method() {
        Call{|GM0127:(
            1,
            2)|};
    }
}
";
            var fixedCode = @"
class Test {
    void Call(int a, int b) { }
    public void Method() {
        Call(1, 2);
    }
}
";
            await CreateTest(testCode, fixedCode).RunAsync();
        }
    }
}
