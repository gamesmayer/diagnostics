namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0008Analyzer>;

    public class GM0008AnalyzerTests
    {
        [Fact]
        public async Task SingleLineInvocation_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        Baz(1, 2);
    }

    void Baz(int a, int b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultilineInvocationWithoutBlankLines_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        Baz(
            1,
            2);
    }

    void Baz(int a, int b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBetweenArguments_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        Baz(
            1,
{|GM0008:
|}            2);
    }

    void Baz(int a, int b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineAfterOpenParen_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        Baz(
{|GM0008:
|}            1,
            2);
    }

    void Baz(int a, int b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodDeclarationBlankLineBetweenParameters_Diagnostic()
        {
            var testCode = @"class Foo
{
    public void Bar(
        int a,
{|GM0008:
|}        int b,
        int c)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleBlankLines_AllDiagnostics()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        Baz(
{|GM0008:
|}            1,
{|GM0008:
|}            2,
            3
{|GM0008:
|}        );
    }

    void Baz(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodDeclarationMultipleBlankLines_AllDiagnostics()
        {
            var testCode = @"class Foo
{
    public void Bar(
{|GM0008:
|}        int a,
{|GM0008:
|}        int b,
        int c
{|GM0008:
|}    )
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodDeclarationMultilineParametersWithoutBlankLines_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Bar(
        int a,
        int b,
        int c)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectCreationBlankLineBetweenArguments_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        var x = new Foo(
            1,
{|GM0008:
|}            2);
    }

    Foo(int a, int b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectCreationMultilineWithoutBlankLines_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        var x = new Foo(
            1,
            2);
    }

    Foo(int a, int b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitObjectCreationBlankLineBetweenArguments_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        Foo x = new(
            1,
{|GM0008:
|}            2);
    }

    Foo(int a, int b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorDeclarationBlankLineBetweenParameters_Diagnostic()
        {
            var testCode = @"class Foo
{
    public Foo(
        int a,
{|GM0008:
|}        int b)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorInitializerThisBlankLineBetweenArguments_Diagnostic()
        {
            var testCode = @"class Foo
{
    public Foo(int a) : this(
        a,
{|GM0008:
|}        false)
    {
    }

    public Foo(int a, bool b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorInitializerBaseBlankLineBetweenArguments_Diagnostic()
        {
            var testCode = @"class Base
{
    public Base(int a, bool b) { }
}

class Foo : Base
{
    public Foo(int a) : base(
        a,
{|GM0008:
|}        false)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorInitializerMultilineWithoutBlankLines_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public Foo(int a) : this(
        a,
        false)
    {
    }

    public Foo(int a, bool b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
