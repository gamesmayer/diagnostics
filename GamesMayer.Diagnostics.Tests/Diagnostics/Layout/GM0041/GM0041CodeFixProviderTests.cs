namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0041CodeFixProviderTests
    {
        [Fact]
        public async Task TwoInvocations_AllSameLine_SingleFix()
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
            var fixedCode = @"
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
            var test = new CSharpCodeFixTest<GM0041Analyzer, GM0041CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ThreeInvocations_AllSameLine_MultipleFix()
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
            var fixedCode = @"
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
            var test = new CSharpCodeFixTest<GM0041Analyzer, GM0041CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task AllInvocationsOnOwnLines_NoFix()
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
            var test = new CSharpCodeFixTest<GM0041Analyzer, GM0041CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task SingleInvocation_NoFix()
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
            var test = new CSharpCodeFixTest<GM0041Analyzer, GM0041CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }
    }
}
