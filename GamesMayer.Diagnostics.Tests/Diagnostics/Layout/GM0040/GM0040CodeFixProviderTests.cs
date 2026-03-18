namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0040CodeFixProviderTests
    {
        [Fact]
        public async Task LastSegmentOnOwnLine_SingleFix()
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
            var test = new CSharpCodeFixTest<GM0040Analyzer, GM0040CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task LastSegmentOnOwnLine_MultipleFix()
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
            var test = new CSharpCodeFixTest<GM0040Analyzer, GM0040CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task AlreadyOnOwnLineWithWrongIndent_AlsoFixed()
        {
            // .ToList() is already on its own line but has extra indentation.
            // The fix should move .Where to its own line AND correct .ToList()'s indentation.
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
            var test = new CSharpCodeFixTest<GM0040Analyzer, GM0040CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task AllSegmentsOnOwnLines_NoFix()
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
            var test = new CSharpCodeFixTest<GM0040Analyzer, GM0040CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task SingleLineChain_NoFix()
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
            var test = new CSharpCodeFixTest<GM0040Analyzer, GM0040CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }
    }
}
