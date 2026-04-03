namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0120Analyzer>;

    public class GM0120AnalyzerTests
    {
        [Fact]
        public async Task ObjectInitializer_NoBlankLines_NoDiagnostic()
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
        public async Task ObjectInitializer_SingleMember_NoDiagnostic()
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
        public async Task ObjectInitializer_BlankLineBetweenMembers_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1,
{|GM0120:
|}            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_MultipleBlankLinesBetweenMembers_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1,
{|GM0120:
|}{|GM0120:
|}            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_BlankLinesBetweenMultiplePairs_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    void M()
    {
        var x = new Foo
        {
            A = 1,
{|GM0120:
|}            B = 2,
{|GM0120:
|}            C = 3
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } public int C { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ImplicitObjectCreation_BlankLineBetweenMembers_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo x = new()
        {
            A = 1,
{|GM0120:
|}            B = 2
        };
    }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ObjectInitializer_AsArgument_BlankLineBetweenMembers_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Use(new Foo
        {
            A = 1,
{|GM0120:
|}            B = 2
        });
    }

    void Use(Foo f) { }
}

class Foo { public int A { get; set; } public int B { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
