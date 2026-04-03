namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0123Analyzer>;

    public class GM0123AnalyzerTests
    {
        [Fact]
        public async Task ObjectInitializer_EachMemberOnOwnLine_NoDiagnostic()
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
        public async Task ObjectInitializer_FirstMemberOnOpenBraceLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        { {|GM0123:A = 1|},
            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_AdjacentMembersOnSameLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1, {|GM0123:B = 2|}
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_MultipleMembersOnSameLine_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1, {|GM0123:B = 2|}, {|GM0123:C = 3|}
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } public int C { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitObjectCreation_EachMemberOnOwnLine_NoDiagnostic()
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
        public async Task ImplicitObjectCreation_AdjacentMembersOnSameLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo x = new()
        {
            A = 1, {|GM0123:B = 2|}
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectAsArgument_EachMemberOnOwnLine_NoDiagnostic()
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
        public async Task ObjectAsArgument_AdjacentMembersOnSameLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new Foo
        {
            A = 1, {|GM0123:B = 2|}
        });
    }

    void Use(Foo f) { }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
