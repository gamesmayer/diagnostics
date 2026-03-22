namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0042Analyzer>;

    public class GM0042AnalyzerTests
    {
        [Fact]
        public async Task NoUsings_NoDiagnostic()
        {
            var testCode = @"class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoMembers_NoDiagnostic()
        {
            var testCode = @"using System;";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineAfterLastUsing_NoDiagnostic()
        {
            var testCode = @"using System;

class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleUsingsWithBlankLine_NoDiagnostic()
        {
            var testCode = @"using System;
using System.Collections;

class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoBlankLineAfterLastUsing_Diagnostic()
        {
            var testCode = @"using System;
{|GM0042:class|} Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleUsingsNoBlankLine_Diagnostic()
        {
            var testCode = @"using System;
using System.Collections;
{|GM0042:class|} Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoBlankLineBeforeNamespace_Diagnostic()
        {
            var testCode = @"using System;
{|GM0042:namespace|} MyApp { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBeforeNamespace_NoDiagnostic()
        {
            var testCode = @"using System;

namespace MyApp { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
