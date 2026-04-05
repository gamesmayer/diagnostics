namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0136Analyzer>;

    public class GM0136AnalyzerTests
    {
        [Fact]
        public async Task SingleLineTypeArgList_NoDiagnostic()
        {
            var testCode = @"
using System.Collections.Generic;

class Foo
{
    void Test()
    {
        var list = new List<int>();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleLineMultipleTypeArgs_NoDiagnostic()
        {
            var testCode = @"
using System.Collections.Generic;

class Foo
{
    void Test()
    {
        var dict = new Dictionary<int, string>();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineTypeArgList_Diagnostic()
        {
            var testCode = @"
using System.Collections.Generic;

class Foo
{
    void Test()
    {
        var dict = new Dictionary{|GM0136:<
            int, string>|}();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineTypeArgList_MethodCall_Diagnostic()
        {
            var testCode = @"
class Foo
{
    static T Create<T>() where T : new() => new T();

    void Test()
    {
        Create{|GM0136:<
            int>|}();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleLineGenericMethodCall_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    static T Create<T>() where T : new() => new T();

    void Test()
    {
        Create<int>();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineThreeTypeArgs_Diagnostic()
        {
            var testCode = @"
class Foo
{
    static void Bar<T1, T2, T3>() { }

    void Test()
    {
        Bar{|GM0136:<
            int,
            string,
            bool>|}();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
