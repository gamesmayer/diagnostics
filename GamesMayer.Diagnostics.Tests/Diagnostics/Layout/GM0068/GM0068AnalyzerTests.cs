namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0068Analyzer>;

    public class GM0068AnalyzerTests
    {
        [Fact]
        public async Task TwoStatements_SeparateLines_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int i = 0;
        System.Console.WriteLine(i);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TwoStatements_SameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int i = 0; {|GM0068:System.Console.WriteLine(i);|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeStatements_TwoOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int i = 0; {|GM0068:int j = 1;|}
        System.Console.WriteLine(i + j);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeStatements_AllOnSameLine_TwoDiagnostics()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int i = 0; {|GM0068:int j = 1;|} {|GM0068:System.Console.WriteLine(i + j);|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleStatement_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        System.Console.WriteLine();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AutoImplementedProperty_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public int Bar { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AutoImplementedPropertyWithOverride_NoDiagnostic()
        {
            var testCode = @"abstract class Base
{
    public abstract int Events { get; set; }
}

class Foo : Base
{
    public override int Events { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
