namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0127Analyzer>;

    public class GM0127AnalyzerTests
    {
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
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
            var test = new CSharpAnalyzerTest<GM0127Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

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
            var test = new CSharpAnalyzerTest<GM0127Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

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
            var test = new CSharpAnalyzerTest<GM0127Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

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
            var test = new CSharpAnalyzerTest<GM0127Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
                MarkupOptions = MarkupOptions.UseFirstDescriptor,
            };
            await test.RunAsync();
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
            var test = new CSharpAnalyzerTest<GM0127Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
                MarkupOptions = MarkupOptions.UseFirstDescriptor,
            };
            await test.RunAsync();
        }
    }
}
