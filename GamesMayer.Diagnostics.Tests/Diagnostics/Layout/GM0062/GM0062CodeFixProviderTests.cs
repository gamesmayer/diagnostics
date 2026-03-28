namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0062CodeFixProviderTests
    {
        [Fact]
        public async Task TwoTypeChecks_Fix()
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
            var fixedCode = @"class EntityA { }
class EntityB { }

class Foo
{
    void M(object n)
    {
        if (n is EntityA or EntityB) { }
    }
}";
            var test = new CSharpCodeFixTest<GM0062Analyzer, GM0062CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ThreeTypeChecks_Fix()
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
            var fixedCode = @"class EntityA { }
class EntityB { }
class EntityC { }

class Foo
{
    void M(object n)
    {
        if (n is EntityA or EntityB or EntityC) { }
    }
}";
            var test = new CSharpCodeFixTest<GM0062Analyzer, GM0062CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
