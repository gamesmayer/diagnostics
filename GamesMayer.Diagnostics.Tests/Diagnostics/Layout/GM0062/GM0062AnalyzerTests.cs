namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0062Analyzer>;

    public class GM0062AnalyzerTests
    {
        [Fact]
        public async Task SingleIsTypeCheck_NoDiagnostic()
        {
            var testCode = @"class EntityA { }

class Foo
{
    void M(object n)
    {
        if (n is EntityA) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IsOrSyntax_NoDiagnostic()
        {
            var testCode = @"class EntityA { }
class EntityB { }

class Foo
{
    void M(object n)
    {
        if (n is EntityA or EntityB) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task DifferentSubjects_NoDiagnostic()
        {
            var testCode = @"class EntityA { }
class EntityB { }

class Foo
{
    void M(object n, object m)
    {
        if (n is EntityA || m is EntityB) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MixedTypeAndNonTypeCheck_NoDiagnostic()
        {
            var testCode = @"class EntityA { }

class Foo
{
    void M(object n, bool condition)
    {
        if (n is EntityA || condition) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TwoTypeChecks_Diagnostic()
        {
            var testCode = @"class EntityA { }
class EntityB { }

class Foo
{
    void M(object n)
    {
        if ({|GM0062:n is EntityA || n is EntityB|}) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ThreeTypeChecks_Diagnostic()
        {
            var testCode = @"class EntityA { }
class EntityB { }
class EntityC { }

class Foo
{
    void M(object n)
    {
        if ({|GM0062:n is EntityA || n is EntityB || n is EntityC|}) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
