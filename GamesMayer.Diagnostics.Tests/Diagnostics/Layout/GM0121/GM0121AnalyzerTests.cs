namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0121Analyzer>;

    public class GM0121AnalyzerTests
    {
        [Fact]
        public async Task InitializerOnSameLine_NoDiagnostic()
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
        public async Task InitializerOnNextLine_NoDiagnostic()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x)
        : base(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThisInitializerOnNextLine_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    public Foo(int x) { }

    public Foo(int x, int y)
        : this(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBetweenDeclarationAndBaseInitializer_Diagnostic()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x)
{|GM0121:|}
        : base(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineBetweenDeclarationAndThisInitializer_Diagnostic()
        {
            var testCode = @"
class Foo
{
    public Foo(int x) { }

    public Foo(int x, int y)
{|GM0121:|}
        : this(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleBlankLines_ReportOnFirstBlankLine()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x)
{|GM0121:|}

        : base(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CommentBetweenDeclarationAndInitializer_NoDiagnostic()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x)
        // initialize with base
        : base(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineAfterColon_Diagnostic()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x) :
{|GM0121:|}
        base(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLineAfterColonThisInitializer_Diagnostic()
        {
            var testCode = @"
class Foo
{
    public Foo(int x) { }

    public Foo(int x, int y) :
{|GM0121:|}
        this(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlankLinesInBothSpans_TwoDiagnostics()
        {
            var testCode = @"
class Bar
{
    public Bar(int x) { }
}

class Foo : Bar
{
    public Foo(int x)
{|GM0121:|}
        :
{|GM0121:|}
        base(x)
    {
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
