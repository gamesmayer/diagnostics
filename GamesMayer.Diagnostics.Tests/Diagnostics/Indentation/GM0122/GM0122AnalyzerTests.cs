namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0122Analyzer>;

    public class GM0122AnalyzerTests
    {
        [Fact]
        public async Task ObjectInitializer_MembersCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1,
            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_SingleLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo { A = 1, B = 2 };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_MemberOnSameLineAsOpenBrace_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        { A = 1,
            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_MembersUnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
        {|GM0122:A|} = 1,
        {|GM0122:B|} = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_MembersOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
                {|GM0122:A|} = 1,
                {|GM0122:B|} = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitObjectCreation_MembersCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo x = new()
        {
            A = 1,
            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitObjectCreation_MembersUnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo x = new()
        {
        {|GM0122:A|} = 1,
        {|GM0122:B|} = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectAsArgument_MembersCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new Foo
        {
            A = 1,
            B = 2
        });
    }

    void Use(Foo f) { }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectAsArgument_MembersUnderIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new Foo
        {
        {|GM0122:A|} = 1,
        {|GM0122:B|} = 2
        });
    }

    void Use(Foo f) { }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldDeclaration_MembersCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    Foo f = new Foo
    {
        A = 1,
        B = 2
    };
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FieldDeclaration_MembersOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    Foo f = new Foo
    {
            {|GM0122:A|} = 1,
            {|GM0122:B|} = 2
    };
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
