namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0061Analyzer>;

    public class GM0061AnalyzerTests
    {
        [Fact]
        public async Task EqualityOperator_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(object x)
    {
        if (x == null) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task InequalityOperator_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(object x)
    {
        if (x != null) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IsTypePattern_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(object x)
    {
        if (x is string s) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IsNull_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(object x)
    {
        if ({|GM0061:x is null|}) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IsNotNull_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(object x)
    {
        if ({|GM0061:x is not null|}) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IsEnumMember_Diagnostic()
        {
            var testCode = @"enum Phase { Began, Ended }

class Foo
{
    void M(Phase phase)
    {
        if ({|GM0061:phase is Phase.Began|}) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IsNotEnumMember_Diagnostic()
        {
            var testCode = @"enum Phase { Began, Ended }

class Foo
{
    void M(Phase phase)
    {
        if ({|GM0061:phase is not Phase.Began|}) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IsConstField_Diagnostic()
        {
            var testCode = @"class Foo
{
    private const int MaxValue = 100;

    void M(int x)
    {
        if ({|GM0061:x is MaxValue|}) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IsOrPattern_WithTypes_NoDiagnostic()
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
        public async Task IsOrPattern_TwoValues_Diagnostic()
        {
            var testCode = @"enum Phase { Began, Moved, Stationary }

class Foo
{
    void M(Phase phase)
    {
        if ({|GM0061:phase is Phase.Moved or Phase.Stationary|}) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IsOrPattern_ThreeValues_Diagnostic()
        {
            var testCode = @"enum Phase { Began, Moved, Stationary, Ended }

class Foo
{
    void M(Phase phase)
    {
        if ({|GM0061:phase is Phase.Moved or Phase.Stationary or Phase.Ended|}) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
