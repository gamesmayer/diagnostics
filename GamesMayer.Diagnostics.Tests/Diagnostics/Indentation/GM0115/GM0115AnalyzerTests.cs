namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0115Analyzer>;

    public class GM0115AnalyzerTests
    {
        [Fact]
        public async Task ConstructorWithBaseInitializerCorrectlyIndented_NoDiagnostic()
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
        public async Task ConstructorWithThisInitializerCorrectlyIndented_NoDiagnostic()
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
        public async Task ConstructorWithBaseInitializerNotIndented_Diagnostic()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x) :
{|GM0115:base|}(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorWithBaseInitializerOverIndented_Diagnostic()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x) :
            {|GM0115:base|}(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorInitializerOnSameLine_NoDiagnostic()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x) : base(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedClassConstructorWithBaseInitializerCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"
class Outer
{
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
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedClassConstructorWithBaseInitializerWronglyIndented_Diagnostic()
        {
            var testCode = @"
class Outer
{
    class Bar
    {
        public Bar(int x) { }
    }

    class Foo : Bar
    {
        public Foo(int x) :
        {|GM0115:base|}(x)
        {
        }
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
