namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0127AnalyzerTests
    {
        private static CSharpAnalyzerTest<GM0127Analyzer, XUnitVerifier> CreateTest(string testCode)
            => new CSharpAnalyzerTest<GM0127Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
                MarkupOptions = MarkupOptions.UseFirstDescriptor,
            };

        [Fact]
        public async Task ThreeArgs_AllOnOneLine_NoDiagnostic()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c) { }
    public void Method() {
        Call(1, 2, 3);
    }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task FourArgs_AllOnOwnLines_NoDiagnostic()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c, int d) { }
    public void Method() {
        Call(
            1,
            2,
            3,
            4);
    }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task FourArgs_AllOnOneLine_ReportsAllArgs()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c, int d) { }
    public void Method() {
        Call({|GM0127:1|}, {|GM0127:2|}, {|GM0127:3|}, {|GM0127:4|});
    }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task FiveArgs_AllOnOneLine_ReportsAllArgs()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c, int d, int e) { }
    public void Method() {
        Call({|GM0127:1|}, {|GM0127:2|}, {|GM0127:3|}, {|GM0127:4|}, {|GM0127:5|});
    }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task FourArgs_FirstOnSameLineAsOpenParen_ReportsFirstArg()
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
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task FourArgs_MixedLines_Diagnostic()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c, int d) { }
    public void Method() {
        Call(
            1,
            2, {|GM0127:3|}, {|GM0127:4|});
    }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task FourArgs_ObjectCreation_Diagnostic()
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
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdTwo_TwoArgs_Diagnostic()
        {
            var testCode = @"
class Test {
    void Call(int a, int b) { }
    public void Method() {
        Call({|GM0127:1|}, {|GM0127:2|});
    }
}
";
            var test = CreateTest(testCode);

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0127.threshold = 2"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdFive_FourArgs_NoDiagnostic()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c, int d) { }
    public void Method() {
        Call(1, 2, 3, 4);
    }
}
";
            var test = CreateTest(testCode);

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0127.threshold = 5"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdFour_FourArgs_Diagnostic()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c, int d) { }
    public void Method() {
        Call({|GM0127:1|}, {|GM0127:2|}, {|GM0127:3|}, {|GM0127:4|});
    }
}
";
            var test = CreateTest(testCode);

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0127.threshold = 4"));

            await test.RunAsync();
        }

        [Fact]
        public async Task OneArg_ComplexExpression_Lambda_AllOnOneLine_Diagnostic()
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
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task OneArg_ComplexExpression_ObjectInitializer_AllOnOneLine_Diagnostic()
        {
            var testCode = @"
class Test {
    void Add(Item item) { }
    public void Method() {
        Add({|GM0127:new Item { Name = ""test"", Value = 42 }|});
    }
}

class Item {
    public string Name { get; set; }
    public int Value { get; set; }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task TwoArgs_SingleLine_NoDiagnostic()
        {
            var testCode = @"
class Test {
    void Call(int a, int b) { }
    public void Method() {
        Call(1, 2);
    }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task TwoArgs_MultiLine_OpenParenOnSameLine_Diagnostic()
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
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task OneArg_MultiLine_OpenParenOnOwnLine_Diagnostic()
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
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task TwoArgs_MultiLine_OpenParenOnOwnLine_Diagnostic()
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
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task TwoArgs_WithLambda_MultiLine_NoDiagnostic()
        {
            var testCode = @"
using System;

class Test {
    void Call(int a, Action<int> b) { }
    public void Method() {
        Call(
            1,
            x => Console.WriteLine(x));
    }
}
";
            await CreateTest(testCode).RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdFive_ThreeArgs_MultiLine_Diagnostic()
        {
            var testCode = @"
class Test {
    void Call(int a, int b, int c) { }
    public void Method() {
        Call{|GM0127:(
            1,
            2,
            3)|};
    }
}
";
            var test = CreateTest(testCode);
            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0127.threshold = 5"));

            await test.RunAsync();
        }
    }
}
