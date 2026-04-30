namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0144CodeFixProviderTests
    {
        [Fact]
        public async Task NotEqualsNull_PropertyAccess_Fix()
        {
            var testCode = @"class C
{
    string M(string x)
    {
        return {|GM0144:x != null ? x.ToString() : null|};
    }
}";
            var fixedCode = @"class C
{
    string M(string x)
    {
        return x?.ToString();
    }
}";
            var test = new CSharpCodeFixTest<GM0144Analyzer, GM0144CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task EqualsNull_PropertyAccess_Fix()
        {
            var testCode = @"class C
{
    string M(string x)
    {
        return {|GM0144:x == null ? null : x.ToString()|};
    }
}";
            var fixedCode = @"class C
{
    string M(string x)
    {
        return x?.ToString();
    }
}";
            var test = new CSharpCodeFixTest<GM0144Analyzer, GM0144CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NotEqualsNull_FieldAccess_Fix()
        {
            var testCode = @"class Foo { public string Value = """"; }
class C
{
    string M(Foo x)
    {
        return {|GM0144:x != null ? x.Value : null|};
    }
}";
            var fixedCode = @"class Foo { public string Value = """"; }
class C
{
    string M(Foo x)
    {
        return x?.Value;
    }
}";
            var test = new CSharpCodeFixTest<GM0144Analyzer, GM0144CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NotEqualsNull_MethodCallWithArgs_Fix()
        {
            var testCode = @"class Foo { public string Get(int n) => """"; }
class C
{
    string M(Foo x)
    {
        return {|GM0144:x != null ? x.Get(1) : null|};
    }
}";
            var fixedCode = @"class Foo { public string Get(int n) => """"; }
class C
{
    string M(Foo x)
    {
        return x?.Get(1);
    }
}";
            var test = new CSharpCodeFixTest<GM0144Analyzer, GM0144CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
