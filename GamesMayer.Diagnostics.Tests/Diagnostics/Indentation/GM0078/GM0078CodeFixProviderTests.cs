namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0078CodeFixProviderTests
    {
        [Fact]
        public async Task OpenBrace_OverIndented_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
        {|GM0078:{|}
        int x = 1;
    }
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CloseBrace_OverIndented_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
        {|GM0078:}|}
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CloseBrace_UnderIndented_Fix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
{|GM0078:}|}
}";
            var fixedCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NestedBlock_InnerOpenBrace_OverIndented_Fix()
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
            var fixedCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
            int x = 1;
        }
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task CorrectlyIndented_NoFix()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = testCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassOpenBrace_OverIndented_Fix()
        {
            var testCode = @"class Foo
    {|GM0078:{|}
}";
            var fixedCode = @"class Foo
{
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ClassCloseBrace_OverIndented_Fix()
        {
            var testCode = @"class Foo
{
    {|GM0078:}|}";
            var fixedCode = @"class Foo
{
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NamespaceOpenBrace_OverIndented_Fix()
        {
            var testCode = @"namespace MyNamespace
    {|GM0078:{|}
    class Foo
    {
    }
}";
            var fixedCode = @"namespace MyNamespace
{
    class Foo
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NamespaceCloseBrace_OverIndented_Fix()
        {
            var testCode = @"namespace MyNamespace
{
    class Foo
    {
    }
    {|GM0078:}|}";
            var fixedCode = @"namespace MyNamespace
{
    class Foo
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task StructOpenBrace_OverIndented_Fix()
        {
            var testCode = @"struct Point
    {|GM0078:{|}
    int X { get; set; }
}";
            var fixedCode = @"struct Point
{
    int X { get; set; }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task StructCloseBrace_OverIndented_Fix()
        {
            var testCode = @"struct Point
{
    int X { get; set; }
    {|GM0078:}|}";
            var fixedCode = @"struct Point
{
    int X { get; set; }
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        // ── Array initializer fix tests (formerly GM0115) ─────────────────────

        [Fact]
        public async Task ArrayInitializer_OpenBrace_OverIndented_Fix()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
        {|GM0078:{|}
        0,
        1
    };
}";
            var fixedCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
    };
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ArrayInitializer_CloseBrace_UnderIndented_Fix()
        {
            var testCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
{|GM0078:}|};
}";
            var fixedCode = @"class C
{
    int[] arr = new int[]
    {
        0,
        1
    };
}";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task LocalVariable_ArrayInitializer_OpenBrace_OverIndented_Fix()
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
            var fixedCode = @"class C
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
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        // ── Object initializer fix tests (formerly GM0121) ────────────────────

        [Fact]
        public async Task ObjectInitializer_OpenBrace_OverIndented_Fix()
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
            var fixedCode = @"class C
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
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ObjectInitializer_CloseBrace_UnderIndented_Fix()
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
            var fixedCode = @"class C
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
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ImplicitObjectCreation_OpenBrace_OverIndented_Fix()
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
            var fixedCode = @"class C
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
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task FieldDeclaration_ObjectInitializer_OpenBrace_OverIndented_Fix()
        {
            var testCode = @"class C
{
    Foo f = new Foo
        {|GM0078:{|}
        A = 1
    };
}

class Foo { public int A { get; set; } }";
            var fixedCode = @"class C
{
    Foo f = new Foo
    {
        A = 1
    };
}

class Foo { public int A { get; set; } }";
            var test = new CSharpCodeFixTest<GM0078Analyzer, GM0078CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
