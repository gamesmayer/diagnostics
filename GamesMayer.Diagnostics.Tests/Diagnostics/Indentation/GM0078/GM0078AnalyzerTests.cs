namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0078Analyzer>;

    public class GM0078AnalyzerTests
    {
        [Fact]
        public async Task CorrectlyIndentedBraces_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyBlock_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OpenBrace_KAndRStyle_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method() {
        int x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OpenBrace_OverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
        {|GM0078:{|}
        int x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CloseBrace_OverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
        {|GM0078:}|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CloseBrace_UnderIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
{|GM0078:}|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BothBraces_WrongIndent_TwoDiagnostics()
        {
            var testCode = @"class Foo
{
    void Method()
        {|GM0078:{|}
        int x = 1;
        {|GM0078:}|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedBlock_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
            int x = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedBlock_InnerBraceWrongIndent_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
            {|GM0078:{|}
            int x = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlockWithDirectives_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
#if SOME_DEFINE
        int x = 1;
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassBraces_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassOpenBrace_OverIndented_Diagnostic()
        {
            var testCode = @"class Foo
    {|GM0078:{|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassCloseBrace_OverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    {|GM0078:}|}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassBraces_KAndRStyle_NoDiagnostic()
        {
            var testCode = @"class Foo {
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamespaceBraces_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"namespace MyNamespace
{
    class Foo
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamespaceOpenBrace_OverIndented_Diagnostic()
        {
            var testCode = @"namespace MyNamespace
    {|GM0078:{|}
    class Foo
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamespaceCloseBrace_OverIndented_Diagnostic()
        {
            var testCode = @"namespace MyNamespace
{
    class Foo
    {
    }
    {|GM0078:}|}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task StructBraces_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"struct Point
{
    int X { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task StructOpenBrace_OverIndented_Diagnostic()
        {
            var testCode = @"struct Point
    {|GM0078:{|}
    int X { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task StructCloseBrace_OverIndented_Diagnostic()
        {
            var testCode = @"struct Point
{
    int X { get; set; }
    {|GM0078:}|}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task StructBraces_KAndRStyle_NoDiagnostic()
        {
            var testCode = @"struct Point {
    int X { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── Array initializer tests (formerly GM0115) ─────────────────────────

        [Fact]
        public async Task ArrayInitializer_BracesCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitArrayInitializer_BracesCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new[]
    {
        0,
        1
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_SingleLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[] { 0, 1 };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_OpenBraceOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
        {|GM0078:{|}
        0,
        1
    };
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_CloseBraceUnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
{|GM0078:}|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrayInitializer_BothBracesWrongIndent_TwoDiagnostics()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
        {|GM0078:{|}
        0,
        1
        {|GM0078:}|};
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LocalVariable_ArrayInitializer_BracesCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int[] arr = new int[]
        {
            0,
            1
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LocalVariable_ArrayInitializer_OpenBraceOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        int[] arr = new int[]
            {|GM0078:{|}
            0,
            1
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NotAnInitialValue_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new int[]
        {
            0,
            1
        });
    }

    void Use(int[] arr) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        // ── Object initializer tests (formerly GM0121) ────────────────────────

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
            {|GM0078:{|}
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
{|GM0078:}|};
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
            {|GM0078:{|}
            A = 1
            {|GM0078:}|};
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
            {|GM0078:{|}
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
            {|GM0078:{|}
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
        {|GM0078:{|}
        A = 1
    };
}

class Foo { public int A { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
