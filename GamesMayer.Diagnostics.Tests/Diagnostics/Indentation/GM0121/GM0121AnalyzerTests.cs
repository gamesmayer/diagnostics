namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0121Analyzer>;

    public class GM0121AnalyzerTests
    {
        [Fact]
        public async Task ObjectInitializer_BracesCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_SingleLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo { A = 1 };
    }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_OpenBraceOnSameLineAsNew_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo {
            A = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_OpenBraceOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
            {|GM0121:{|}
            A = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_CloseBraceUnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1
{|GM0121:}|};
    }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_BothBracesWrongIndent_TwoDiagnostics()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
            {|GM0121:{|}
            A = 1
            {|GM0121:}|};
    }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitObjectCreation_BracesCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo x = new()
        {
            A = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitObjectCreation_OpenBraceOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo x = new()
            {|GM0121:{|}
            A = 1
        };
    }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectAsArgument_BracesCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new Foo
        {
            A = 1
        });
    }

    void Use(Foo f) { }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectAsArgument_OpenBraceOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new Foo
            {|GM0121:{|}
            A = 1
        });
    }

    void Use(Foo f) { }
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldDeclaration_BracesCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    Foo f = new Foo
    {
        A = 1
    };
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldDeclaration_OpenBraceOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    Foo f = new Foo
        {|GM0121:{|}
        A = 1
    };
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
