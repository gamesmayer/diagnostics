namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0035CodeFixProviderTests
    {
        [Fact]
        public async Task BlankLineBetweenFluentSegments_Fix()
        {
            var testCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
            .Where(x => x > 1)
{|GM0035:
|}            .Select(x => x * 2)
            .ToList();
    }
}";
            var fixedCode = @"using System.Linq;

class C
{
    void M()
    {
        var values = new[] { 1, 2, 3 }
            .Where(x => x > 1)
            .Select(x => x * 2)
            .ToList();
    }
}";

            var test = new CSharpCodeFixTest<GM0035Analyzer, GM0035CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task BlankLineBeforeFirstFluentSegment_Fix()
        {
            var testCode = @"class C
{
    CharacterDefinitionRepository characterDefinitionRepository;

    void M(string key)
    {
        var definition = characterDefinitionRepository
{|GM0035:
|}            .Find(key);
    }
}

class CharacterDefinitionRepository
{
    public object Find(string key) => new object();
}";
            var fixedCode = @"class C
{
    CharacterDefinitionRepository characterDefinitionRepository;

    void M(string key)
    {
        var definition = characterDefinitionRepository
            .Find(key);
    }
}

class CharacterDefinitionRepository
{
    public object Find(string key) => new object();
}";

            var test = new CSharpCodeFixTest<GM0035Analyzer, GM0035CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
