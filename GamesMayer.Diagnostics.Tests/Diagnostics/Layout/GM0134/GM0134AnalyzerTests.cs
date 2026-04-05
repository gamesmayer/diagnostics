namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0134Analyzer>;

    public class GM0134AnalyzerTests
    {
        [Fact]
        public async Task NoBlankLinesAroundColon_Class_NoDiagnostic()
        {
            var testCode = @"
interface IBar { }

class Foo :
    IBar
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBeforeColon_Diagnostic()
        {
            var testCode = @"
interface IBar { }

class Foo
{|GM0134:|}
    : IBar
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineAfterColon_Diagnostic()
        {
            var testCode = @"
interface IBar { }

class Foo :
{|GM0134:|}
    IBar
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLinesBeforeAndAfterColon_TwoDiagnostics()
        {
            var testCode = @"
interface IFoo { }

interface IBar
{|GM0134:|}
    :
{|GM0134:|}
    IFoo
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}