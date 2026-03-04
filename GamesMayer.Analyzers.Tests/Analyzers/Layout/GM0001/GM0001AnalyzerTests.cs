namespace GamesMayer.Analyzers.Tests.Analyzers.Layout.GM0001
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Analyzers.GM0001Analyzer>;

    public class GM0001AnalyzerTests
    {
        [Fact]
        public async Task SingleUsing_NoDiagnostic()
        {
            var testCode = @"using System;
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleUsingsNoBlankLine_NoDiagnostic()
        {
            var testCode = @"using System;
using System.Collections;
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBetweenUsings_Diagnostic()
        {
            var testCode = @"using System;
{|GM0001:
|}using System.Collections;
class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
