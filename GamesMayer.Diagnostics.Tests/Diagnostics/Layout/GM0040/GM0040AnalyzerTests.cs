namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0040Analyzer>;

    public class GM0040AnalyzerTests
    {
        [Fact]
        public async Task SingleLineChain_NoDiagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items.Where(x => x > 1).Select(x => x * 2).ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AllSegmentsOnOwnLines_NoDiagnostic()
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
        public async Task AllmanStyleInvocation_AllSegmentsOnOwnLines_NoDiagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items
            .Where
            (
                x => x > 1
            )
            .ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LastSegmentOnOwnLine_FirstSegmentNot_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items{|GM0040:.Where(x => x > 1)|}
            .ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LastSegmentOnOwnLine_MultipleSegmentsNot_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items{|GM0040:.Where(x => x > 1)|}{|GM0040:.Select(x => x * 2)|}
            .ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FirstSegmentOnOwnLine_OtherSegmentsNot_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items
            .Where(x => x > 1){|GM0040:.Select(x => x * 2)|}{|GM0040:.ToList()|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AllmanStyleSegmentOnOwnLine_OtherSegmentsNot_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items{|GM0040:.Where(x => x > 1)|}{|GM0040:.Select
            (
                x => x * 2
            )|}
            .ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonInvocationPrefixOnSameLine_OnlyFirstInvocationReported_Diagnostic()
        {
            // .Property is a non-invocation member access; reporting starts from the first invocation (.Where)
            var testCode = @"
using System.Linq;
class Wrapper {
    public System.Collections.Generic.List<int> Items { get; } = new System.Collections.Generic.List<int> { 1, 2, 3 };
}
class Test {
    public void Method() {
        var wrapper = new Wrapper();
        var result = wrapper.Items{|GM0040:.Where(x => x > 1)|}
            .ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonInvocationPrefixAllOnOwnLines_NoDiagnostic()
        {
            var testCode = @"
using System.Linq;
class Outer {
    class Inner {
        public System.Collections.Generic.List<int> Items => new System.Collections.Generic.List<int> { 1, 2, 3 };
    }
    public void Method() {
        var obj = new Inner();
        var result = obj.Items
            .Where(x => x > 1)
            .ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyAccessBetweenInvocations_PropertyNotReported_Diagnostic()
        {
            // .GetItems() is invocation 1 (not on own line), .Values is a property access (not reported),
            // .Where(...) is invocation 2 (not on own line). Only invocations are reported.
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
        var result = source{|GM0040:.GetContainer()|}.Values{|GM0040:.Where(x => x > 1)|}
            .ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ReturnStatement_MixedLines_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public System.Collections.Generic.List<int> Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        return items{|GM0040:.Where(x => x > 1)|}
            .ToList();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrowExpression_MixedLines_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    private System.Collections.Generic.List<int> _items = new System.Collections.Generic.List<int> { 1, 2, 3 };
    public System.Collections.Generic.List<int> Result => _items{|GM0040:.Where(x => x > 1)|}
        .ToList();
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
