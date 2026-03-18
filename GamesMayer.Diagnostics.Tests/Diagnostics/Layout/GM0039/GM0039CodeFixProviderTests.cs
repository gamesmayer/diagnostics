namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0039CodeFixProviderTests
    {
        [Fact]
        public async Task DotAtEndOfLine_SingleFix()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items{|GM0039:.
            Where(x => x > 1)|};
    }
}
";
            var fixedCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items
            .Where(x => x > 1);
    }
}
";
            var test = new CSharpCodeFixTest<GM0039Analyzer, GM0039CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task DotAtEndOfLine_MultipleFix()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items{|GM0039:.
            Where(x => x > 1)|}{|GM0039:.
            Select(x => x * 2)|}{|GM0039:.
            ToList()|};
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
            var test = new CSharpCodeFixTest<GM0039Analyzer, GM0039CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                NumberOfFixAllIterations = 1,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ExpressionStatement_NoFix()
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
            var test = new CSharpCodeFixTest<GM0039Analyzer, GM0039CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ArrowExpressionProperty_NoFix()
        {
            var testCode = @"
using System.Linq;
class Test {
    public System.Collections.Generic.List<int> Items => 
        new System.Collections.Generic.List<int> { 1, 2, 3 };

    public System.Collections.Generic.List<int> FilteredItems => Items
        .Where(x => x > 1)
        .ToList();
}
";
            var test = new CSharpCodeFixTest<GM0039Analyzer, GM0039CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }
    }
}
