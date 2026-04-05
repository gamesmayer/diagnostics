namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0041Analyzer>;

    public class GM0041AnalyzerTests
    {
        [Fact]
        public async Task SingleInvocation_NoDiagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items.Where(x => x > 1);
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TwoInvocations_AllOnOwnLines_NoDiagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items
            .Where(x => x > 1)
            .ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeInvocations_AllOnOwnLines_NoDiagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items
            .Where(x => x > 1)
            .Select(x => x * 2)
            .ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TwoInvocations_MixedLines_NoDiagnostic()
        {
            // Mixed layout (some on own line, some not) is handled by GM0040, not GM0041
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items.Where(x => x > 1)
            .ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TwoInvocations_AllSameLine_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items{|GM0041:.Where(x => x > 1)|}{|GM0041:.ToList()|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeInvocations_AllSameLine_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items{|GM0041:.Where(x => x > 1)|}{|GM0041:.Select(x => x * 2)|}{|GM0041:.ToList()|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ReturnStatement_AllSameLine_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public System.Collections.Generic.List<int> Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        return items{|GM0041:.Where(x => x > 1)|}{|GM0041:.ToList()|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrowExpression_AllSameLine_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    private System.Collections.Generic.List<int> _items = new System.Collections.Generic.List<int> { 1, 2, 3 };
    public System.Collections.Generic.List<int> Result => _items{|GM0041:.Where(x => x > 1)|}{|GM0041:.ToList()|};
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionStatement_AllSameLine_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        items{|GM0041:.Where(x => x > 1)|}{|GM0041:.ToList()|}{|GM0041:.GetEnumerator()|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedChainInArgument_AllSameLine_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = System.Linq.Enumerable.ToList(items{|GM0041:.Where(x => x > 1)|}{|GM0041:.Select(x => x * 2)|});
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyAccessSubchainPlusNextInvocation_SegmentDiagnostic()
        {
            var testCode = @"
using System.Linq;
class Container {
    public System.Collections.Generic.List<int> Values { get; } = new System.Collections.Generic.List<int> { 1, 2, 3 };
}
class Source {
    public Container GetContainer() => new Container();
}
class Test {
    public void Method() {
        var source = new Source();
        var result = source{|GM0041:.GetContainer()|}{|GM0041:.Values.Where(x => x > 1)|}{|GM0041:.ToList()|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DefaultThreshold_TwoInvocations_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items{|GM0041:.Where(x => x > 1)|}{|GM0041:.ToList()|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConfiguredThresholdThree_TwoInvocations_NoDiagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items.Where(x => x > 1).ToList();
    }
}
";

            var test = new CSharpAnalyzerTest<GM0041Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0041.threshold = 3"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdThree_ThreeInvocations_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items{|GM0041:.Where(x => x > 1)|}{|GM0041:.Select(x => x * 2)|}{|GM0041:.ToList()|};
    }
}
";

            var test = new CSharpAnalyzerTest<GM0041Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0041.threshold = 3"));

            await test.RunAsync();
        }

        [Fact]
        public async Task SingleInvocation_OnOwnLine_Diagnostic()
        {
            // Single invocation is below the default threshold of 2, so it must stay on the same line
            var testCode = @"
class Test {
    static void Method() {
        Installer
            {|GM0041:.Install()|};
    }
}
class Installer {
    public static void Install() {}
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConfiguredThresholdThree_TwoInvocations_AllOnOwnLines_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items
            {|GM0041:.Where(x => x > 1)|}
            {|GM0041:.ToList()|};
    }
}
";

            var test = new CSharpAnalyzerTest<GM0041Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0041.threshold = 3"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdThree_TwoInvocations_OnSameLine_NoDiagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items.Where(x => x > 1).ToList();
    }
}
";

            var test = new CSharpAnalyzerTest<GM0041Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0041.threshold = 3"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ConfiguredThresholdThree_ThreeInvocations_AllOnOwnLines_NoDiagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items
            .Where(x => x > 1)
            .Select(x => x * 2)
            .ToList();
    }
}
";

            var test = new CSharpAnalyzerTest<GM0041Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0041.threshold = 3"));

            await test.RunAsync();
        }
    }
}
