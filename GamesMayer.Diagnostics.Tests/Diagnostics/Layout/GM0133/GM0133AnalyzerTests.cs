namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0133Analyzer>;

    public class GM0133AnalyzerTests
    {
        [Fact]
        public async Task ConstructorWithBaseInitializerOnOwnLine_NoDiagnostic()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x) :
        base(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorWithThisInitializerOnOwnLine_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    public Foo(int x) { }

    public Foo(int x, int y) :
        this(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorWithBaseInitializerOnDeclarationLine_Diagnostic()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x) : {|GM0133:base|}(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorWithThisInitializerOnDeclarationLine_Diagnostic()
        {
            var testCode = @"
class Foo
{
    public Foo(int x) { }

    public Foo(int x, int y) : {|GM0133:this|}(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
