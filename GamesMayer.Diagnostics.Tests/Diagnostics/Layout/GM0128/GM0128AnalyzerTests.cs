namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0128Analyzer>;

    public class GM0128AnalyzerTests
    {
        [Fact]
        public async Task NewAndTypeOnSameLine_NoDiagnostic()
        {
            var testCode = @"
class Foo { }
class Test {
    public void Method() {
        var x = new Foo();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NewAndTypeOnDifferentLines_Diagnostic()
        {
            var testCode = @"
class Foo { }
class Test {
    public void Method() {
        var x = new
            {|GM0128:Foo|}();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NewAndGenericTypeOnSameLine_NoDiagnostic()
        {
            var testCode = @"
using System.Collections.Generic;
class Test {
    public void Method() {
        var x = new List<int>();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NewAndGenericTypeOnDifferentLines_Diagnostic()
        {
            var testCode = @"
using System.Collections.Generic;
class Test {
    public void Method() {
        var x = new
            {|GM0128:List<int>|}();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
