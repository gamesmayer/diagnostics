namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0039Analyzer>;

    public class GM0039AnalyzerTests
    {
        [Fact]
        public async Task SingleLineChain_NoDiagnostic()
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
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DotOnNextLine_NoDiagnostic()
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
        public async Task DotAtEndOfLine_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public void Method() {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        var result = items{|GM0039:.
            Where(x => x > 1)|}{|GM0039:.
            ToList()|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DotAtEndOfLineMultipleTimes_Diagnostic()
        {
            var testCode = @"
using System.Linq;
class Test {
    public System.Collections.Generic.List<int> GetValues()
    {
        var items = new System.Collections.Generic.List<int> { 1, 2, 3 };
        return items{|GM0039:.
            Where(x => x > 1)|}{|GM0039:.
            Select(x => x * 2)|}{|GM0039:.
            ToList()|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
